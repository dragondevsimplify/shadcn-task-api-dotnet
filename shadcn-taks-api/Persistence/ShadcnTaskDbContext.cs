using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Entities;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Persistence;

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
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(1000).IsRequired();
            entity.HasMany(x => x.Tags).WithMany(x => x.Tasks).UsingEntity<TaskTag>();
        });
    }
}