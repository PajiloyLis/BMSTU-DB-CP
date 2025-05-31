using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Context.Configuration;

public class ScoreDbConfiguration : IEntityTypeConfiguration<ScoreDb>
{
    public void Configure(EntityTypeBuilder<ScoreDb> builder)
    {
        builder.ToTable("score_story");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(x => x.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasColumnName("position_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(x => x.EfficiencyScore)
            .HasColumnName("efficiency_score")
            .IsRequired();

        builder.Property(x => x.EngagementScore)
            .HasColumnName("engagement_score")
            .IsRequired();

        builder.Property(x => x.CompetencyScore)
            .HasColumnName("competency_score")
            .IsRequired();

        // Foreign key relationships
        builder.HasOne<EmployeeDb>()
            .WithMany(e => e.Scores)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<EmployeeDb>()
            .WithMany(e => e.AuthoredScores)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<PositionDb>()
            .WithMany(p => p.Scores)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Check constraints
        builder.HasCheckConstraint("CK_ScoreStory_EfficiencyScore", "efficiency_score > 0 AND efficiency_score < 6");
        builder.HasCheckConstraint("CK_ScoreStory_EngagementScore", "engagement_score > 0 AND engagement_score < 6");
        builder.HasCheckConstraint("CK_ScoreStory_CompetencyScore", "competency_score > 0 AND competency_score < 6");
    }
}