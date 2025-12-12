using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TaskEntity"/> entity.
/// Sets primary key, property constraints, and relationships with User, TaskList, and Tag.
/// </summary>
public class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    /// <summary>
    /// Configures the <see cref="TaskEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        // Set primary key
        builder.HasKey(t => t.Id);

        // Configure properties
        builder.Property(t => t.Id).HasColumnType("uuid");
        builder.Property(t => t.OwnerId).HasColumnType("uuid");
        builder.Property(t => t.TagId).HasColumnType("uuid");
        builder.Property(t => t.TaskListId).HasColumnType("uuid");
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.Status).IsRequired();

        // Configure relationship with User
        builder.HasOne(t => t.Owner)
            .WithMany(u => u.OwnedTasks)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with TaskList
        builder.HasOne(t => t.TaskList)
            .WithMany(tl => tl.Tasks)
            .HasForeignKey(t => t.TaskListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Tag
        builder.HasOne(t => t.Tag)
            .WithMany(t => t.Tasks)
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
