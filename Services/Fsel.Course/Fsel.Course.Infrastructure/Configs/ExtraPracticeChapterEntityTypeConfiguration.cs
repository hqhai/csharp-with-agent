// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExtraPracticeChapterEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeChapter>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeChapter> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ExtraPractice)
                .WithMany(b => b.ExtraPracticeChapters)
                .HasForeignKey(b => b.ExtraPracticeId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
