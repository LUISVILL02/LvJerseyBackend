using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.Configurations;

public class EmailVerificationConfiguration: IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.ToTable("Email_verifications");

        builder.HasKey(e => e.Id).HasName("Email_verifications_pkey");
        
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Code).HasColumnType("character varying").HasColumnName("code");
        builder.Property(e => e.ExpiryAt).HasColumnType("timestamp with timezone").HasColumnName("expiry_at");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        
        builder.HasOne(d => d.User)
            .WithMany(p => p.EmailVerifications)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_EmailVerifications_Users");
    }
}