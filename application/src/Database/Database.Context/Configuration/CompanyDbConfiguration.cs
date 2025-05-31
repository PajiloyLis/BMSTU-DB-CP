using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Context.Configuration;

/// <summary>
/// Company configuration.
/// </summary>
public class CompanyDbConfiguration : IEntityTypeConfiguration<CompanyDb>
{
    public void Configure(EntityTypeBuilder<CompanyDb> builder)
    {
        builder.ToTable("company");

        builder.HasKey(keyExpression => keyExpression.Id);
        builder.Property(keyExpression => keyExpression.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(keyExpression => keyExpression.Title).HasColumnName("title").HasColumnType("text").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.Title).IsUnique();

        builder.Property(keyExpression => keyExpression.RegistrationDate).HasColumnName("registration_date").HasColumnType("date").IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("RegistrationDateCheck", "registration_date <= CURRENT_DATE"));

        builder.Property(keyExpression => keyExpression.PhoneNumber).HasColumnName("phone").HasColumnType("varchar(16)").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.PhoneNumber).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("PhoneNumberCheck", "phone ~ '^\\+[0-9]{1,3}[0-9]{4,14}$'"));

        builder.Property(keyExpression => keyExpression.Email).HasColumnName("email").HasColumnType("varchar(255)").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.Email).IsUnique();
        builder.ToTable(t =>
            t.HasCheckConstraint("EmailCheck", "email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'"));

        builder.Property(keyExpression => keyExpression.Inn).HasColumnName("inn").HasColumnType("varchar(10)").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.Inn).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("InnCheck", "inn ~ '^[0-9]{10}$'"));

        builder.Property(keyExpression => keyExpression.Kpp).HasColumnName("kpp").HasColumnType("varchar(9)").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.Kpp).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("KppCheck", "kpp ~ '^[0-9]{9}$'"));

        builder.Property(keyExpression => keyExpression.Ogrn).HasColumnName("ogrn").HasColumnType("varchar(13)").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.Ogrn).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("OgrnChek", "ogrn ~ '^[0-9]{13}$'"));

        builder.Property(keyExpression => keyExpression.Address).HasColumnName("address").HasColumnType("text").IsRequired();

        builder.HasMany<PostDb>(c => c.Posts)
            .WithOne()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany<PositionDb>(c => c.Positions)
            .WithOne()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}