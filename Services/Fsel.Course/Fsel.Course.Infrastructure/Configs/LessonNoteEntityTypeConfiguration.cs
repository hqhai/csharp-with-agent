// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonNoteEntityTypeConfiguration : IEntityTypeConfiguration<LessonNote>
    {
        public void Configure(EntityTypeBuilder<LessonNote> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.LessonResult)
                 .WithMany(b => b.LessonNotes)
                 .HasForeignKey(p => p.LessonResultId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Type)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumNoteType>());
        }
    }
}
