using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Context.Configuration;

public class EducationDbConfiguration : IEntityTypeConfiguration<EducationDb>
{
    public void Configure(EntityTypeBuilder<EducationDb> builder)
    {
        builder.ToTable("education");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(e => e.Institution)
            .HasColumnName("institution")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Level)
            .HasColumnName("education_level")
            .HasColumnType("text")
            .IsRequired();

        builder.HasCheckConstraint("education_level_check",
            "education_level in ('Высшее (бакалавриат)', 'Высшее (магистратура)', 'Высшее (специалитет)', 'Среднее профессиональное (ПКР)', 'Среднее профессиональное (ПССЗ)','Программы переподготовки', 'Курсы повышения квалификации' )");

        builder.Property(e => e.StudyField)
            .HasColumnName("study_field")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date");

        builder.HasCheckConstraint("start_date_check", "start_date < CURRENT_DATE");

        builder.HasCheckConstraint("end_date_check", "end_date < CURRENT_DATE");

        builder.HasOne<EmployeeDb>()
            .WithMany(e => e.Educations)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}