// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameVocabularyEntityTypeConfiguration : IEntityTypeConfiguration<GameVocabulary>
    {
        public void Configure(EntityTypeBuilder<GameVocabulary> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CefrLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumGameCefrLevel>());
            builder.Property(e => e.CourseLevel)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumGameCourseLevel>());
            builder.Property(e => e.UnitOrder)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumUnitOrder>());
            builder.Property(e => e.PartSpeech)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumPartSpeech>());
            builder.HasOne(a => a.GameCenter)
               .WithMany(b => b.GameVocabularies)
               .HasForeignKey(b => b.GameCenterId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
