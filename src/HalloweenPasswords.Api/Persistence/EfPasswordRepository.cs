using HalloweenPasswords.Api.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HalloweenPasswords.Api.Persistence;

public sealed class EfPasswordRepository(PasswordsDbContext dbContext) : IPasswordRepository
{
    public async Task AddAsync(Guid id, string value, CancellationToken cancellationToken = default)
    {
        dbContext.Passwords.Add(new StoredPassword
        {
            Id = id,
            Value = value
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<string?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Passwords
            .AsNoTracking()
            .Where(password => password.Id == id)
            .Select(password => password.Value)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
