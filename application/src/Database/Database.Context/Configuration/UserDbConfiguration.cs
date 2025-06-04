using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Context.Configuration;

public class UserDbConfiguration : IEntityTypeConfiguration<UserDb>
{
    public void Configure(EntityTypeBuilder<UserDb> builder)
    {
        builder.ToTable("users");

        builder.HasKey(keyExpression => keyExpression.Email);
        builder.Property(keyExpression => keyExpression.Email).HasColumnName("email").HasColumnType("nvarchar").IsRequired();
        builder.ToTable(t =>
            t.HasCheckConstraint("EmailCheck", "email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'"));

        builder.Property(keyExpression => keyExpression.Password).HasColumnName("password").HasColumnType("text").IsRequired();
        builder.HasIndex(keyExpression => keyExpression.Password).IsUnique();
        
        builder.Property(u => u.Salt).HasColumnName("salt").HasColumnType("text").IsRequired();
        
        builder.Property(u => u.Role).HasColumnName("role").HasColumnType("text").IsRequired();
    }
}