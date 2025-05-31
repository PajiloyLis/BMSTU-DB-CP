// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
//
// namespace Database.Context;
//
// public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProjectDbContext>
// {
//     public ProjectDbContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
//         optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=project;Username=postgres;Password=postgres");
//
//         return new ProjectDbContext(optionsBuilder.Options);
//     }
// } 