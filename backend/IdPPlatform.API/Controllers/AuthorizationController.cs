using System.Security.Claims;
using System.Text.Json;
using IdPPlatform.API.Common;
using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.Oidc;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

public sealed class AuthorizationController : Controller
{
    private readonly IAuthSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOidcClientValidator _clientValidator;
    private readonly IOidcAuthorizationService _authorizationService;
    private readonly IOidcTokenService _tokenService;
    private readonly IOidcClaimsService _claimsService;

    public AuthorizationController(
        IAuthSessionRepository sessions,
        IUnitOfWork unitOfWork,
        IOidcClientValidator clientValidator,
        IOidcAuthorizationService authorizationService,
        IOidcTokenService tokenService,
        IOidcClaimsService claimsService)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _clientValidator = clientValidator;
        _authorizationService = authorizationService;
        _tokenService = tokenService;
        _claimsService = claimsService;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        var request = ReadAuthorizeRequest();

        var (client, clientError) = await _clientValidator.ValidateClientAsync(request.ClientId, null, cancellationToken);
        if (clientError is not null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, clientError);
        }

        var redirectError = _clientValidator.ValidateRedirectUri(client!, request.RedirectUri);
        if (redirectError is not null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, redirectError);
        }

        var scopes = _clientValidator.ParseScopes(request.Scope);
        var scopeError = _clientValidator.ValidateScopes(client!, scopes);
        if (scopeError is not null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, scopeError);
        }

        var pkceError = _clientValidator.ValidatePkceForAuthorize(client!, request.CodeChallenge, request.CodeChallengeMethod);
        if (pkceError is not null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, pkceError);
        }

        var clientContext = new ApplicationClientValidationContext { Client = client!, Scopes = scopes };
        var requestError = _authorizationService.ValidateAuthorizeRequest(request, clientContext);
        if (requestError is not null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, requestError);
        }

        var cookieAuth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var prompt = request.Prompt ?? string.Empty;
        if (!cookieAuth.Succeeded ||
            prompt.Contains("login", StringComparison.OrdinalIgnoreCase) ||
            (request.MaxAge is not null && cookieAuth.Properties?.IssuedUtc is not null &&
             DateTimeOffset.UtcNow - cookieAuth.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            if (prompt.Contains("none", StringComparison.OrdinalIgnoreCase))
            {
                return OAuthRedirectError(request.RedirectUri, request.State, new OidcError
                {
                    Error = OidcConstants.Errors.LoginRequired,
                    ErrorDescription = "Interactive login is required."
                });
            }

            return Challenge(
                new AuthenticationProperties { RedirectUri = BuildAuthorizeReturnUrl() },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        var login = ReadLoginFromPrincipal(cookieAuth.Principal!);
        var session = await _sessions.GetForUpdateAsync(login.SessionId, cancellationToken);
        if (session is null || session.Status != SessionStatus.Active)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, new OidcError
            {
                Error = OidcConstants.Errors.LoginRequired,
                ErrorDescription = "Session is no longer active."
            });
        }

        if (!session.ClientId.HasValue)
        {
            session.BindOAuthClient(client!.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var claims = await _claimsService.TryBuildClaimsAsync(session.Id, scopes, cancellationToken);
        if (claims is null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, new OidcError
            {
                Error = OidcConstants.Errors.LoginRequired,
                ErrorDescription = "Unable to build token claims."
            });
        }

        var (code, codeError) = await _authorizationService.CreateAuthorizationCodeAsync(
            request,
            session.Id,
            client!.Id,
            scopes,
            cancellationToken);
        if (codeError is not null)
        {
            return OAuthRedirectError(request.RedirectUri, request.State, codeError);
        }

        var redirect = QueryString.Create(new Dictionary<string, string?>
        {
            ["code"] = code,
            ["state"] = request.State
        }.Where(x => !string.IsNullOrWhiteSpace(x.Value)));

        return Redirect($"{request.RedirectUri}{redirect}");
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
    {
        var form = await Request.ReadFormAsync(cancellationToken);
        var request = new OidcTokenRequest
        {
            GrantType = form["grant_type"].ToString(),
            Code = form["code"].ToString(),
            RedirectUri = form["redirect_uri"].ToString(),
            ClientId = form["client_id"].ToString(),
            ClientSecret = form["client_secret"].ToString(),
            CodeVerifier = form["code_verifier"].ToString(),
            RefreshToken = form["refresh_token"].ToString(),
            Scope = form["scope"].ToString()
        };

        var (response, error) = await _tokenService.ExchangeAsync(request, cancellationToken);
        if (error is not null)
        {
            return OAuthJsonError(error);
        }

        return Ok(response);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public IActionResult Userinfo()
    {
        return Ok(new
        {
            sub = User.FindFirst(OidcConstants.Claims.Subject)?.Value,
            email = User.FindFirst(OidcConstants.Claims.Email)?.Value,
            name = User.FindFirst(OidcConstants.Claims.Name)?.Value,
            tid = User.FindFirst("tid")?.Value,
            mid = User.FindFirst("mid")?.Value,
            trole = User.FindAll("trole").Select(c => c.Value).ToArray(),
            prole = User.FindFirst("prole")?.Value
        });
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout([FromQuery] string? post_logout_redirect_uri)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!string.IsNullOrWhiteSpace(post_logout_redirect_uri))
        {
            return Redirect(post_logout_redirect_uri);
        }

        return Redirect("/");
    }

    private OidcAuthorizeRequest ReadAuthorizeRequest()
    {
        string Read(string key) =>
            Request.HasFormContentType ? Request.Form[key].ToString() : Request.Query[key].ToString();

        return new OidcAuthorizeRequest
        {
            ClientId = Read("client_id"),
            RedirectUri = Read("redirect_uri"),
            ResponseType = Read("response_type"),
            Scope = Read("scope"),
            State = NullIfEmpty(Read("state")),
            Prompt = NullIfEmpty(Read("prompt")),
            MaxAge = int.TryParse(Read("max_age"), out var maxAge) ? maxAge : null,
            CodeChallenge = NullIfEmpty(Read("code_challenge")),
            CodeChallengeMethod = NullIfEmpty(Read("code_challenge_method")),
            Nonce = NullIfEmpty(Read("nonce"))
        };
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private string BuildAuthorizeReturnUrl()
    {
        var query = Request.HasFormContentType
            ? Request.Form.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value.ToString()))
            : Request.Query.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value.ToString()));

        return Request.PathBase + Request.Path + QueryString.Create(query);
    }

    private static OidcLoginContext ReadLoginFromPrincipal(ClaimsPrincipal principal)
    {
        var loginJson = principal.FindFirstValue("idp_login");
        if (string.IsNullOrWhiteSpace(loginJson))
        {
            throw new InvalidOperationException("Login context is missing from the authentication cookie.");
        }

        return JsonSerializer.Deserialize<OidcLoginContext>(loginJson)
            ?? throw new InvalidOperationException("Login context is invalid.");
    }

    private IActionResult OAuthRedirectError(string? redirectUri, string? state, OidcError error)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return OAuthJsonError(error);
        }

        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["error"] = error.Error,
            ["error_description"] = error.ErrorDescription,
            ["state"] = state
        }.Where(x => !string.IsNullOrWhiteSpace(x.Value)));

        return Redirect($"{redirectUri}{query}");
    }

    private IActionResult OAuthJsonError(OidcError error)
    {
        return BadRequest(new { error = error.Error, error_description = error.ErrorDescription });
    }
}
