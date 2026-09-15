using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.HasKey(x => x.AppUserId);
        b.Property(x => x.UserName).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.UserName).IsUnique();
        b.Property(x => x.Role).HasMaxLength(20).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
