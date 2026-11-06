namespace HalloweenPasswords.Api.Application.Abstractions;

public interface IPasswordRepository
{
    Task AddAsync(Guid id, string value, CancellationToken cancellationToken = default);

    Task<string?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
