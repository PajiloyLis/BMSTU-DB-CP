using Database.Context.Configuration;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Project.Core.Models;
using Project.Database.Context.Configuration;
using Project.Database.Models;

namespace Database.Context;

/// <summary>
/// Database context.
/// </summary>
public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options)
    {
    }
    
    protected ProjectDbContext(){}

    public DbSet<EmployeeDb> EmployeeDb { get; set; }

    public DbSet<CompanyDb> CompanyDb { get; set; }

    public DbSet<EducationDb> EducationDb { get; set; }

    public DbSet<PostDb> PostDb { get; set; }

    public DbSet<PositionDb> PositionDb { get; set; }

    public DbSet<ScoreDb> ScoreDb { get; set; }

    public DbSet<PostHistoryDb> PostHistoryDb { get; set; }

    public DbSet<PositionHistoryDb> PositionHistoryDb { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PositionHierarchyWithEmployeeIdDb>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null);
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
        });

        modelBuilder.Entity<PositionHierarchyDb>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
                entity.Property(e => e.Level).HasColumnName("level");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.PositionId).HasColumnName("id");
                entity.Property(e => e.ParentId).HasColumnName("parent_id");
            }
        );
        
        modelBuilder.HasDbFunction(
            typeof(ProjectDbContext)
                .GetMethod(nameof(GetCurrentSubordinatesIdByEmployeeId))!);

        modelBuilder.HasDbFunction(() => GetCurrentSubordinatesIdByEmployeeId(Guid.Empty));
        
        modelBuilder.ApplyConfiguration(new EmployeeDbConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyDbConfiguration());
        modelBuilder.ApplyConfiguration(new EducationDbConfiguration());
        modelBuilder.ApplyConfiguration(new PostDbConfiguration());
        modelBuilder.ApplyConfiguration(new PositionDbConfiguration());
        modelBuilder.ApplyConfiguration(new PostHistoryDbConfiguration());
        modelBuilder.ApplyConfiguration(new ScoreDbConfiguration());
        modelBuilder.ApplyConfiguration(new PositionHistoryDbConfiguration());
    }
    
    [DbFunction(Name = "get_current_subordinates_id_by_employee_id", Schema = "public")]
    public IQueryable<PositionHierarchyWithEmployeeIdDb> GetCurrentSubordinatesIdByEmployeeId(Guid startId)
    {
        return Set<PositionHierarchyWithEmployeeIdDb>()
            .FromSqlRaw("SELECT * FROM get_current_subordinates_id_by_employee_id({0})", startId);
    }
    
    [DbFunction(Name = "get_subordinates_by_id", Schema = "public")]
    public IQueryable<PositionHierarchyDb> GetSubordinatesById(Guid startId)
    {
        return Set<PositionHierarchyDb>()
            .FromSqlRaw("SELECT * FROM get_subordinates_by_id({0})", startId);
    }
}