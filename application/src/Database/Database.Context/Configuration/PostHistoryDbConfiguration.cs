using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Models;
using Project.Core.Models.PostHistory;

namespace Database.Context.Configuration;

public class PostHistoryDbConfiguration : IEntityTypeConfiguration<PostHistoryDb>
{
    public void Configure(EntityTypeBuilder<PostHistoryDb> builder)
    {
        builder.ToTable("post_history");

        builder.HasKey(ph => new { ph.PostId, ph.EmployeeId});

        builder.Property(ph => ph.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(ph => ph.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(ph => ph.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(ph => ph.EndDate)
            .HasColumnName("end_date").IsRequired(false);

        builder.HasCheckConstraint(
            "CK_post_history_start_date",
            "start_date < CURRENT_DATE");

        builder.HasCheckConstraint(
            "CK_post_history_end_date",
            "end_date <= CURRENT_DATE");

        builder.HasOne<PostDb>()
            .WithMany(p => p.PostHistories)
            .HasForeignKey(ph => ph.PostId)
            .HasPrincipalKey(p => p.Id)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<EmployeeDb>()
            .WithMany(e => e.PostHistories)
            .HasForeignKey(ph => ph.EmployeeId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}