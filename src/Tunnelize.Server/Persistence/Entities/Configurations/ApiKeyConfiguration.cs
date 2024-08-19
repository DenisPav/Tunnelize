using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tunnelize.Server.Persistence.Entities.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKey");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SubDomain)
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(x => x.Key)
            .HasMaxLength(50)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();
        builder.HasOne(x => x.User)
            .WithMany(x => x.ApiKeys)
            .HasForeignKey(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.Key)
            .IsUnique();
    }
}