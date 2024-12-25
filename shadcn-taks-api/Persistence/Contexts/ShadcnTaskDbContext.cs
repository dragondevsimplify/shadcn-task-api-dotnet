using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Converters;
using shadcn_taks_api.Persistence.Entities;
using Task = shadcn_taks_api.Persistence.Entities.Task;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;
using TaskPriority = shadcn_taks_api.Persistence.Entities.TaskPriority;

namespace shadcn_taks_api.Persistence.Contexts;

public class ShadcnTaskDbContext(DbContextOptions<ShadcnTaskDbContext> options) : DbContext
{
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;
        const string connectionString =
            "Server=.;Database=ShadcnTaskApi;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(1000).IsRequired();
            entity.HasMany(x => x.Tags).WithMany(x => x.Tasks).UsingEntity<TaskTag>();
            entity.Property(x => x.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => EnumConverter.ConvertToEnum<TaskStatus>(v))
                .HasMaxLength(100).IsRequired();
            entity.Property(x => x.Priority)
                .HasConversion(
                    v => v.ToString(),
                    v => EnumConverter.ConvertToEnum<TaskPriority>(v))
                .HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });
    }
}