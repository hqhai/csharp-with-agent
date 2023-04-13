// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VideoTimeCodeAnswerEntityTypeConfiguration : IEntityTypeConfiguration<VideoTimeCodeAnswer>
    {
        public void Configure(EntityTypeBuilder<VideoTimeCodeAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.VideoResult)
              .WithMany(b => b.VideoTimeCodeAnswers)
              .HasForeignKey(b => b.VideoResultId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.VideoTimeCode)
                          .WithMany(b => b.VideoTimeCodeAnswers)
                          .HasForeignKey(b => b.VideoTimeCodeId)
                          .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Exercise)
              .WithMany(b => b.VideoTimeCodeAnswers)
              .HasForeignKey(b => b.ExerciseId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Question)
                   .WithMany(b => b.VideoTimeCodeAnswers)
                   .HasForeignKey(b => b.QuestionId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
