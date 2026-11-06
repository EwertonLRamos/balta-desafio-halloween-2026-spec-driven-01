using HalloweenPasswords.Api.Application.Abstractions;
using HalloweenPasswords.Api.Application.Models;

namespace HalloweenPasswords.Api.Application.Services;

public sealed class PasswordService(
    IPasswordGenerator passwordGenerator,
    IPasswordRepository passwordRepository) : IPasswordService
{
    public async Task<GeneratedPassword> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var value = passwordGenerator.Generate();

        await passwordRepository.AddAsync(id, value, cancellationToken);

        return new GeneratedPassword(id, value);
    }

    public Task<string?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return passwordRepository.GetValueByIdAsync(id, cancellationToken);
    }
}
