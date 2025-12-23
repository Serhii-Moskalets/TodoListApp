using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="CommentEntity"/> entity.
/// Sets primary key, property constraints, and relationships.
/// </summary>
public class CommentEntityConfiguration : IEntityTypeConfiguration<CommentEntity>
{
    /// <summary>
    /// Configures the <see cref="CommentEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<CommentEntity> builder)
    {
        // Set primary key
        builder.HasKey(c => c.Id);

        // Configure properties
        builder.Property(c => c.Id).HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(c => c.UserId).HasColumnType("uuid");
        builder.Property(c => c.TaskId).HasColumnType("uuid");
        builder.Property(c => c.Text).IsRequired().HasMaxLength(4000);

        // Configure relationship with User
        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Task
        builder.HasOne(t => t.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
