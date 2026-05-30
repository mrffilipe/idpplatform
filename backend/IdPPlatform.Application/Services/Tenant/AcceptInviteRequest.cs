using System.Text.Json.Serialization;

namespace IdPPlatform.Application.Services.Tenant;

public sealed record AcceptInviteRequest
{
    [JsonPropertyName("token")]
    public required string InviteToken { get; init; }

    public Guid ActorUserId { get; init; }
}
