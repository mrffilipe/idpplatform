namespace IdPPlatform.API.Models;

public sealed class AccountLoginViewModel
{
    public string? ReturnUrl { get; init; }

    public bool ShowLocalLogin { get; init; }

    public IReadOnlyList<FederatedProviderViewModel> FederatedProviders { get; init; } = [];
}

public sealed class FederatedProviderViewModel
{
    public required string Alias { get; init; }

    public required string DisplayName { get; init; }

    public required string ProviderType { get; init; }

    public IReadOnlyDictionary<string, string>? ClientConfig { get; init; }
}
