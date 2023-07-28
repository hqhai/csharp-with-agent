// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
