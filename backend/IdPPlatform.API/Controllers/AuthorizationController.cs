using System.Security.Claims;
using System.Text.Json;
using IdPPlatform.API.Common;
using IdPPlatform.API.Models.Oidc;
using IdPPlatform.Application.Services.Oidc;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

/// <summary>
/// OAuth 2.0 authorization server and OpenID Connect provider endpoints under /connect.
/// </summary>
[ApiExplorerSettings(GroupName = "oidc")]
[Tags("OAuth 2.0 / OpenID Connect")]
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

    /// <summary>
    /// OAuth 2.0 authorization endpoint (authorization code + PKCE).
    /// </summary>
    /// <remarks>
    /// Accepts the same parameters via query string (GET) or form body (POST). On success, redirects to
    /// <c>redirect_uri</c> with <c>code</c> and <c>state</c>. On failure, redirects with <c>error</c> and
    /// <c>error_description</c>, or returns a JSON error when <c>redirect_uri</c> is invalid.
    /// </remarks>
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(OidcErrorJsonResponse), StatusCodes.Status400BadRequest)]
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
                    ErrorDescription = ApiErrorMessages.OidcLogin.InteractiveLoginRequired
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
                ErrorDescription = ApiErrorMessages.OidcLogin.SessionNoLongerActive
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
                ErrorDescription = ApiErrorMessages.OidcLogin.UnableToBuildClaims
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

    /// <summary>
    /// OAuth 2.0 token endpoint (authorization_code and refresh_token grants).
    /// </summary>
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OidcTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OidcErrorJsonResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OidcTokenResponse>> Exchange(
        [FromForm] OidcTokenFormRequest form,
        CancellationToken cancellationToken)
    {
        var request = new OidcTokenRequest
        {
            GrantType = form.GrantType,
            Code = form.Code,
            RedirectUri = form.RedirectUri,
            ClientId = form.ClientId,
            ClientSecret = form.ClientSecret,
            CodeVerifier = form.CodeVerifier,
            RefreshToken = form.RefreshToken,
            Scope = form.Scope
        };

        var (response, error) = await _tokenService.ExchangeAsync(request, cancellationToken);
        if (error is not null)
        {
            return OAuthJsonError(error);
        }

        return Ok(response);
    }

    /// <summary>
    /// OpenID Connect UserInfo endpoint (requires Bearer access token).
    /// </summary>
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OidcUserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<OidcUserInfoResponse> Userinfo()
    {
        return Ok(new OidcUserInfoResponse
        {
            Sub = User.FindFirst(OidcConstants.Claims.Subject)?.Value,
            Email = User.FindFirst(OidcConstants.Claims.Email)?.Value,
            Name = User.FindFirst(OidcConstants.Claims.Name)?.Value,
            Tid = User.FindFirst("tid")?.Value,
            Mid = User.FindFirst("mid")?.Value,
            Trole = User.FindAll("trole").Select(c => c.Value).ToArray(),
            Prole = User.FindFirst("prole")?.Value
        });
    }

    /// <summary>
    /// Ends the browser session (cookie) and optionally redirects to the client post-logout URI.
    /// </summary>
    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Logout([FromQuery(Name = "post_logout_redirect_uri")] string? postLogoutRedirectUri)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri))
        {
            return Redirect(postLogoutRedirectUri);
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
            throw new InvalidOperationException(ApiErrorMessages.OidcLogin.MissingLoginContext);
        }

        return JsonSerializer.Deserialize<OidcLoginContext>(loginJson)
            ?? throw new InvalidOperationException(ApiErrorMessages.OidcLogin.InvalidLoginContext);
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

    private ActionResult OAuthJsonError(OidcError error) =>
        BadRequest(new OidcErrorJsonResponse
        {
            Error = error.Error,
            ErrorDescription = error.ErrorDescription
        });
}
