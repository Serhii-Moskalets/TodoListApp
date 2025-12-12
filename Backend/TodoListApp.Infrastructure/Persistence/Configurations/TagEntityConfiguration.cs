using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TagEntity"/> entity.
/// Sets primary key, property constraints, and relationships.
/// </summary>
public class TagEntityConfiguration : IEntityTypeConfiguration<TagEntity>
{
    /// <summary>
    /// Configures the <see cref="TagEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        // Set primary key
        builder.HasKey(t => t.Id);

        // Configure properties
        builder.Property(t => t.Name).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Id).HasColumnType("uuid");
        builder.Property(t => t.UserId).HasColumnType("uuid");

        // Configure relationship with User
        builder.HasOne(t => t.User)
            .WithMany(t => t.Tags)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
