
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TaskItem.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Index compus pentru filtrarea după Status și DueDate
        builder.HasIndex(t => new { t.Status, t.DueDate })
            .HasDatabaseName("IX_Tasks_Status_DueDate");

        // Index individual pentru filtrarea doar după DueDate
        builder.HasIndex(t => t.DueDate);

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Tags)
            .WithMany(tag => tag.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskItemTags",
                right => right.HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("TagId"),
                left => left.HasOne<TaskItem>()
                    .WithMany()
                    .HasForeignKey("TaskItemId"),
                join => join.HasKey("TaskItemId", "TagId"));
    }
}
