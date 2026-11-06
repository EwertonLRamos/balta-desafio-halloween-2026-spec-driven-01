using HalloweenPasswords.Api.Application.Models;

namespace HalloweenPasswords.Api.Application.Services;

public interface IPasswordService
{
    Task<GeneratedPassword> GenerateAsync(CancellationToken cancellationToken = default);

    Task<string?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
