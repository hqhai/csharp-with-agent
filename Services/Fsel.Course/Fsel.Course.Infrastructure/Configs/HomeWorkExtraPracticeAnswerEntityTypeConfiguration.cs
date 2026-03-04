// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HomeWorkExtraPracticeAnswerEntityTypeConfiguration : IEntityTypeConfiguration<HomeWorkExtraPracticeAnswer>
    {
        public void Configure(EntityTypeBuilder<HomeWorkExtraPracticeAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.HomeWorkExtraPracticeResult)
              .WithMany(b => b.HomeWorkExtraPracticeAnswers)
              .HasForeignKey(b => b.HomeWorkExtraPracticeResultId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Question)
              .WithMany(b => b.HomeWorkExtraPracticeAnswers)
              .HasForeignKey(b => b.QuestionId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumAnswerStatus>());

            builder.HasIndex(c => new { c.QuestionId, c.HomeWorkExtraPracticeResultId }).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
