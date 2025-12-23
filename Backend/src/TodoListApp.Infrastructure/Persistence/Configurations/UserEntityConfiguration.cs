using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="UserEntity"/> entity.
/// Sets primary key, property constraints, and unique indexes.
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <summary>
    /// Configures the <see cref="UserEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // Set primary key
        builder.HasKey(u => u.Id);

        // Configure properties
        builder.Property(u => u.Id).HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(20);
        builder.Property(u => u.LastName).HasMaxLength(30);
        builder.Property(u => u.UserName).IsRequired().HasMaxLength(20);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.PendingEmail).HasMaxLength(100);
        builder.Property(u => u.TokenType).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
