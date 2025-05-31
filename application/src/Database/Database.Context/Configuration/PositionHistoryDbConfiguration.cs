using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Database.Models;

namespace Project.Database.Context.Configuration;

public class PositionHistoryDbConfiguration : IEntityTypeConfiguration<PositionHistoryDb>
{
    public void Configure(EntityTypeBuilder<PositionHistoryDb> builder)
    {
        builder.ToTable("position_history");

        builder.HasKey(x => new { x.PositionId, x.EmployeeId });

        builder.Property(x => x.PositionId)
            .HasColumnName("position_id")
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date").IsRequired(false);

        builder.HasCheckConstraint("CK_position_history_start_date", "start_date < CURRENT_DATE");
        builder.HasCheckConstraint("CK_position_history_end_date", "end_date <= CURRENT_DATE");

        builder.HasOne<PositionDb>()
            .WithMany(p => p.PositionHistories)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<EmployeeDb>()
            .WithMany(e => e.PositionHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}