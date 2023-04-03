// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonNoteEntityTypeConfiguration : IEntityTypeConfiguration<LessonNote>
    {
        public void Configure(EntityTypeBuilder<LessonNote> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.LessonResult)
                 .WithOne(b => b.LessonNote)
                 .HasForeignKey<LessonNote>(p => p.LessonResultId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
