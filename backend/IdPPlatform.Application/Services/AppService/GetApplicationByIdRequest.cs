namespace IdPPlatform.Application.Services.AppService;

public sealed record GetApplicationByIdRequest
{
    public required Guid ApplicationId { get; init; }
}
