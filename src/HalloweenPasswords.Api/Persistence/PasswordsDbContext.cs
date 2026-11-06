using Microsoft.EntityFrameworkCore;

namespace HalloweenPasswords.Api.Persistence;

public sealed class PasswordsDbContext(DbContextOptions<PasswordsDbContext> options)
    : DbContext(options)
{
    public DbSet<StoredPassword> Passwords => Set<StoredPassword>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredPassword>(entity =>
        {
            entity.HasKey(password => password.Id);
            entity.Property(password => password.Value).IsRequired();
        });
    }
}
