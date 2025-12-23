using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="UserTaskAccessEntity"/> entity.
/// Sets composite primary key and relationships with <see cref="UserEntity"/> and <see cref="TaskEntity"/>.
/// </summary>
public class UserTaskAccessEntityConfiguration : IEntityTypeConfiguration<UserTaskAccessEntity>
{
    /// <summary>
    /// Configures the <see cref="UserTaskAccessEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserTaskAccessEntity> builder)
    {
        // Set composite primary key
        builder.HasKey(uta => new { uta.UserId, uta.TaskId });

        // Configure properties
        builder.Property(uta => uta.UserId).HasColumnType("uuid");
        builder.Property(uta => uta.TaskId).HasColumnType("uuid");

        // Configure relationship with User
        builder.HasOne(uta => uta.User)
            .WithMany(u => u.TaskAccesses)
            .HasForeignKey(uta => uta.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Task
        builder.HasOne(uta => uta.Task)
            .WithMany(t => t.UserAccesses)
            .HasForeignKey(uta => uta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
