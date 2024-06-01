using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tunnelize.Server.Persistence.Entities.Configurations;

public class UserCodeConfiguration : IEntityTypeConfiguration<UserCode>
{
    public void Configure(EntityTypeBuilder<UserCode> builder)
    {
        builder.ToTable("UserCode");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .HasMaxLength(6)
            .IsRequired();
        
        builder.Property(x => x.Expiration)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Codes)
            .HasForeignKey(x => x.UserId)
            .IsRequired();
    }
}