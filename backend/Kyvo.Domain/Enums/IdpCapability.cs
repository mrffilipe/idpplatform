namespace Kyvo.Domain.Enums;

/// <summary>
/// Authentication capabilities that an <see cref="IdentityProviderType"/> can advertise.
/// A single platform may enable several providers; uniqueness is enforced for sensitive
/// capabilities (e.g., <see cref="LocalPassword"/>) and warned for socials.
/// </summary>
public enum IdpCapability
{
    LocalPassword = 0,
    GoogleSocial = 1,
    MicrosoftSocial = 2,
    AppleSocial = 3,
    GenericOidc = 4
}
