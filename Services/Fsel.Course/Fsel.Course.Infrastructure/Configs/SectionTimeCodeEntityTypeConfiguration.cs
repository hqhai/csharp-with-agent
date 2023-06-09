// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class SectionTimeCodeEntityTypeConfiguration : IEntityTypeConfiguration<SectionTimeCode>
    {
        public void Configure(EntityTypeBuilder<SectionTimeCode> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Section)
                .WithMany(b => b.SectionTimeCodes)
                .HasForeignKey(b => b.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
