// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VideoTimeCodeAnswerEntityTypeConfiguration : IEntityTypeConfiguration<VideoTimeCodeAnswer>
    {
        public void Configure(EntityTypeBuilder<VideoTimeCodeAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                  .HasMaxLength(100)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumTimeCodeStatus>());

            builder.HasOne(a => a.VideoTimeCodeResult)
                          .WithMany(b => b.VideoTimeCodeAnswers)
                          .HasForeignKey(b => b.VideoTimeCodeResultId)
                          .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Exercise)
              .WithMany(b => b.VideoTimeCodeAnswers)
              .HasForeignKey(b => b.ExerciseId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Question)
                   .WithMany(b => b.VideoTimeCodeAnswers)
                   .HasForeignKey(b => b.QuestionId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.VideoResult)
                  .WithMany(b => b.VideoTimeCodeAnswers)
                  .HasForeignKey(b => b.VideoResultId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.VideoTimeCode)
                          .WithMany(b => b.VideoTimeCodeAnswers)
                          .HasForeignKey(b => b.VideoTimeCodeId)
                          .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
