namespace Kyvo.API.Models;

/// <summary>
/// Minimal response body when an endpoint creates a resource and returns its identifier.
/// </summary>
public sealed record CreatedIdResponse(Guid Id);
