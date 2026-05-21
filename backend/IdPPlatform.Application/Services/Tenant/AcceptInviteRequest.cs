namespace IdPPlatform.Application.Services.Tenant;

public sealed record AcceptInviteRequest
{
    public required string InviteToken { get; init; }

    public required Guid ActorUserId { get; init; }
}
