using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Context.Configuration;

public class PostDbConfiguration : IEntityTypeConfiguration<PostDb>
{
    public void Configure(EntityTypeBuilder<PostDb> builder)
    {
        builder.ToTable("post");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()").ValueGeneratedNever().IsRequired();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.Salary)
            .HasColumnName("salary")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(p => p.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid").ValueGeneratedNever()
            .IsRequired();

        builder.HasCheckConstraint("salary_check", "salary > 0");

        builder.HasOne<CompanyDb>()
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}