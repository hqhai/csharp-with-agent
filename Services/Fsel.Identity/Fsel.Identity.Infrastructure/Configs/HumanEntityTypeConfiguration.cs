// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HumanEntityTypeConfiguration : IEntityTypeConfiguration<Human>
    {
        public void Configure(EntityTypeBuilder<Human> builder)
        {
            builder.HasOne<User>(x => x.User).WithOne(b => b.Human)
                .HasForeignKey<Human>(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
