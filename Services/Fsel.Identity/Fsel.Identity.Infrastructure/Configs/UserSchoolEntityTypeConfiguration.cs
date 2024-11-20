// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class UserSchoolEntityTypeConfiguration : IEntityTypeConfiguration<UserSchool>
    {
        public void Configure(EntityTypeBuilder<UserSchool> builder)

        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.User)
                 .WithMany(b => b.UserSchools)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.UserId).IsUnique(false);
        }
    }
}
