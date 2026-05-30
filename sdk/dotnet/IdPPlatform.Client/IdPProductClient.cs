using System.Net.Http.Headers;
using System.Net.Http.Json;
using IdPPlatform.Client.Internal;
using IdPPlatform.Client.Models;
using Microsoft.Extensions.Options;

namespace IdPPlatform.Client;

public sealed class IdPProductClient : IIdPProductClient
{
    private readonly HttpClient _http;
    private readonly IdPPlatformClientOptions _options;

    public IdPProductClient(HttpClient http, IOptions<IdPPlatformClientOptions> options)
    {
        _http = http;
        _options = options.Value;
        Auth = new AuthApi(this);
        Users = new UsersApi(this);
        Tenants = new TenantsApi(this);
        Memberships = new MembershipsApi(this);
        TenantRoles = new TenantRolesApi(this);
        AuditLogs = new AuditLogsApi(this);
    }

    public IIdPAuthApi Auth { get; }
    public IIdPUsersApi Users { get; }
    public IIdPTenantsApi Tenants { get; }
    public IIdPMembershipsApi Memberships { get; }
    public IIdPTenantRolesApi TenantRoles { get; }
    public IIdPAuditLogsApi AuditLogs { get; }

    private string V => _options.VersionPrefix;

    private async Task<HttpResponseMessage> SendAsync(
        string accessToken,
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            message.Content = JsonContent.Create(body, options: IdPHttpResponse.SerializerOptions);
        }

        return await _http.SendAsync(message, cancellationToken);
    }

    private sealed class AuthApi(IdPProductClient client) : IIdPAuthApi
    {
        public async Task<SubscribeTenantResult> SubscribeAsync(
            string userAccessToken,
            SubscribeTenantRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Post,
                $"{client.V}/auth/subscribe",
                request,
                cancellationToken);

            var data = await IdPHttpResponse.ReadJsonAsync<SubscribeTenantResponse>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP subscribe returned empty body.");

            var context = new TenantContextResult(
                data.UserId,
                data.Email,
                data.TenantId,
                data.MembershipId,
                data.TenantRoles,
                data.PlatformRoles,
                data.Tenants);

            return new SubscribeTenantResult(context, data.Tokens);
        }

        public async Task<TenantContextResult> SwitchTenantAsync(
            string userAccessToken,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Post,
                $"{client.V}/auth/switch-tenant",
                new SwitchTenantRequest(tenantId),
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<TenantContextResult>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP switch-tenant returned empty body.");
        }

        public async Task<IReadOnlyList<AuthSessionDto>> ListSessionsAsync(
            string userAccessToken,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/auth/sessions",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<List<AuthSessionDto>>(response, cancellationToken) ?? [];
        }

        public async Task RevokeSessionAsync(
            string userAccessToken,
            Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Delete,
                $"{client.V}/auth/sessions/{sessionId}",
                null,
                cancellationToken);

            await IdPHttpResponse.EnsureSuccessAsync(response, cancellationToken);
        }
    }

    private sealed class UsersApi(IdPProductClient client) : IIdPUsersApi
    {
        public async Task<UserDto> GetMeAsync(string userAccessToken, CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(userAccessToken, HttpMethod.Get, $"{client.V}/Users/me", null, cancellationToken);
            return await IdPHttpResponse.ReadJsonAsync<UserDto>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP Users/me returned empty body.");
        }

        public async Task UpdateMeAsync(
            string userAccessToken,
            UpdateUserProfileBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(userAccessToken, HttpMethod.Patch, $"{client.V}/Users/me", body, cancellationToken);
            await IdPHttpResponse.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<PagedResult<UserMembershipDto>> ListMyMembershipsAsync(
            string userAccessToken,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/Users/me/memberships?page={page}&pageSize={pageSize}",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<PagedResult<UserMembershipDto>>(response, cancellationToken)
                ?? new PagedResult<UserMembershipDto>([], page, pageSize, 0);
        }
    }

    private sealed class TenantsApi(IdPProductClient client) : IIdPTenantsApi
    {
        public async Task<PagedResult<TenantDto>> ListAsync(
            string userAccessToken,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/Tenants?page={page}&pageSize={pageSize}",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<PagedResult<TenantDto>>(response, cancellationToken)
                ?? new PagedResult<TenantDto>([], page, pageSize, 0);
        }

        public async Task<TenantDto> GetByIdAsync(
            string userAccessToken,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/Tenants/{tenantId}",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<TenantDto>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP tenant not found.");
        }

        public async Task UpdateAsync(
            string userAccessToken,
            Guid tenantId,
            UpdateTenantBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Patch,
                $"{client.V}/Tenants/{tenantId}",
                body,
                cancellationToken);

            await IdPHttpResponse.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<CreatedIdResponse> InviteMemberAsync(
            string userAccessToken,
            Guid tenantId,
            InviteMemberBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Post,
                $"{client.V}/Tenants/{tenantId}/invites",
                body,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<CreatedIdResponse>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP invite returned empty body.");
        }

        public async Task<CreatedMembershipIdResponse> AcceptInviteAsync(
            string userAccessToken,
            AcceptInviteBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Post,
                $"{client.V}/invites/accept",
                body,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<CreatedMembershipIdResponse>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP accept invite returned empty body.");
        }
    }

    private sealed class MembershipsApi(IdPProductClient client) : IIdPMembershipsApi
    {
        public async Task<CreatedIdResponse> CreateAsync(
            string userAccessToken,
            Guid tenantId,
            CreateMembershipBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Post,
                $"{client.V}/tenants/{tenantId}/memberships",
                body,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<CreatedIdResponse>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP create membership returned empty body.");
        }

        public async Task<PagedResult<MembershipDto>> ListByTenantAsync(
            string userAccessToken,
            Guid tenantId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/tenants/{tenantId}/memberships?page={page}&pageSize={pageSize}",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<PagedResult<MembershipDto>>(response, cancellationToken)
                ?? new PagedResult<MembershipDto>([], page, pageSize, 0);
        }

        public async Task UpdateRolesAsync(
            string userAccessToken,
            Guid membershipId,
            UpdateMembershipRolesBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Patch,
                $"{client.V}/Memberships/{membershipId}",
                body,
                cancellationToken);

            await IdPHttpResponse.EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task RevokeAsync(
            string userAccessToken,
            Guid membershipId,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Delete,
                $"{client.V}/Memberships/{membershipId}",
                null,
                cancellationToken);

            await IdPHttpResponse.EnsureSuccessAsync(response, cancellationToken);
        }
    }

    private sealed class TenantRolesApi(IdPProductClient client) : IIdPTenantRolesApi
    {
        public async Task<PagedResult<TenantRoleDto>> ListAsync(
            string userAccessToken,
            Guid tenantId,
            bool includeInactive = false,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/tenants/{tenantId}/roles?includeInactive={includeInactive}&page={page}&pageSize={pageSize}",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<PagedResult<TenantRoleDto>>(response, cancellationToken)
                ?? new PagedResult<TenantRoleDto>([], page, pageSize, 0);
        }

        public async Task<CreatedIdResponse> CreateAsync(
            string userAccessToken,
            Guid tenantId,
            CreateTenantRoleBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Post,
                $"{client.V}/tenants/{tenantId}/roles",
                body,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<CreatedIdResponse>(response, cancellationToken)
                ?? throw new InvalidOperationException("IdP create role returned empty body.");
        }

        public async Task UpdateAsync(
            string userAccessToken,
            Guid roleId,
            UpdateTenantRoleBody body,
            CancellationToken cancellationToken = default)
        {
            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Patch,
                $"{client.V}/TenantRoles/{roleId}",
                body,
                cancellationToken);

            await IdPHttpResponse.EnsureSuccessAsync(response, cancellationToken);
        }
    }

    private sealed class AuditLogsApi(IdPProductClient client) : IIdPAuditLogsApi
    {
        public async Task<PagedResult<AuditLogItemDto>> ListAsync(
            string userAccessToken,
            ListAuditLogsQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            query ??= new ListAuditLogsQuery();
            var qs = new List<string>
            {
                $"page={query.Page}",
                $"pageSize={query.PageSize}"
            };
            if (!string.IsNullOrWhiteSpace(query.Action)) qs.Add($"action={Uri.EscapeDataString(query.Action)}");
            if (query.From.HasValue) qs.Add($"from={Uri.EscapeDataString(query.From.Value.ToString("O"))}");
            if (query.To.HasValue) qs.Add($"to={Uri.EscapeDataString(query.To.Value.ToString("O"))}");

            var response = await client.SendAsync(
                userAccessToken,
                HttpMethod.Get,
                $"{client.V}/AuditLogs?{string.Join("&", qs)}",
                null,
                cancellationToken);

            return await IdPHttpResponse.ReadJsonAsync<PagedResult<AuditLogItemDto>>(response, cancellationToken)
                ?? new PagedResult<AuditLogItemDto>([], query.Page, query.PageSize, 0);
        }
    }
}
