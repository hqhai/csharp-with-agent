// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SectionPartEntityTypeConfiguration : IEntityTypeConfiguration<SectionPart>
    {
        public void Configure(EntityTypeBuilder<SectionPart> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Section)
                .WithMany(b => b.SectionParts)
                .HasForeignKey(b => b.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
