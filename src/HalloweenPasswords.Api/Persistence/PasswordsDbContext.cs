using Microsoft.EntityFrameworkCore;

namespace HalloweenPasswords.Api.Persistence;

public sealed class PasswordsDbContext(DbContextOptions<PasswordsDbContext> options)
    : DbContext(options)
{
}
