using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TaskListEntity"/> entity.
/// Sets primary key, property constraints, and relationship with User.
/// </summary>
public class TaskListEntityConfiguration : IEntityTypeConfiguration<TaskListEntity>
{
    /// <summary>
    /// Configures the <see cref="TaskListEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TaskListEntity> builder)
    {
        // Set primary key
        builder.HasKey(tl => tl.Id);

        // Configure properties
        builder.Property(tl => tl.Id).HasColumnType("uuid");
        builder.Property(tl => tl.OwnerId).HasColumnType("uuid");
        builder.Property(tl => tl.Title).IsRequired().HasMaxLength(50);

        // Configure relationship with User
        builder.HasOne(tl => tl.Owner)
            .WithMany(o => o.TaskLists)
            .HasForeignKey(tl => tl.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
