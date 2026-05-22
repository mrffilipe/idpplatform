using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.Registration;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;

namespace IdPPlatform.Infrastructure.Services.Registration;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _users;
    private readonly IUserCredentialRepository _userCredentials;
    private readonly IIdentityProviderRepository _identityProviders;
    private readonly IPasswordPolicy _passwordPolicy;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(
        IUserRepository users,
        IUserCredentialRepository userCredentials,
        IIdentityProviderRepository identityProviders,
        IPasswordPolicy passwordPolicy,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _userCredentials = userCredentials;
        _identityProviders = identityProviders;
        _passwordPolicy = passwordPolicy;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new DomainValidationException(ApplicationErrorMessages.Registration.EmailRequired);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new DomainValidationException(ApplicationErrorMessages.Registration.DisplayNameRequired);
        }

        var passwordResult = _passwordPolicy.Validate(request.Password);
        if (!passwordResult.IsValid)
        {
            throw new DomainValidationException(string.Join(" ", passwordResult.Errors));
        }

        // Self-registration only makes sense when at least one local IdP advertises LocalPassword.
        var localProviders = await _identityProviders.ListEnabledByCapabilityAsync(IdpCapability.LocalPassword, cancellationToken);
        if (localProviders.Count == 0)
        {
            throw new DomainBusinessRuleException(ApplicationErrorMessages.Registration.LocalPasswordDisabled);
        }

        var normalizedEmail = request.Email.Trim();
        if (await _users.EmailAlreadyExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new DomainBusinessRuleException(ApplicationErrorMessages.Registration.EmailAlreadyExists);
        }

        var user = new User(new EmailAddress(normalizedEmail), request.DisplayName.Trim());
        await _users.AddAsync(user, cancellationToken);

        var credential = new UserCredential(user.Id, BCrypt.Net.BCrypt.HashPassword(request.Password));
        await _userCredentials.AddAsync(credential, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult
        {
            UserId = user.Id,
            Email = user.Email.Value,
            DisplayName = user.DisplayName
        };
    }
}
