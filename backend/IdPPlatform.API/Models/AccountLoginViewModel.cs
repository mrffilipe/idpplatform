namespace IdPPlatform.API.Models;

public sealed class AccountLoginViewModel
{
    public string? ReturnUrl { get; init; }

    public bool ShowLocalLogin { get; init; }

    public IReadOnlyList<FederatedProviderViewModel> FederatedProviders { get; init; } = [];
}
