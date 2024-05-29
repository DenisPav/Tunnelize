using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tunnelize.Server.Persistence.Entities.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Email)
            .IsRequired();
        
        builder.Property(x => x.PasswordHash)
            .IsRequired();
        
        builder.HasData(new User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            PasswordHash = "AQAAAAIAAYagAAAAENDbkRIeCytZXQTBw4dWGgoFvlkq2yZKfx9n1OtFNQr+rHBkbMa2T5Zi2NlCuEm6zw=="
        });
    }
}