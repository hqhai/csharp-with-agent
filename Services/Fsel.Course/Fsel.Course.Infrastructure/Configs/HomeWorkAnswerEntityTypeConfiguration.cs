// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HomeWorkAnswerEntityTypeConfiguration : IEntityTypeConfiguration<HomeWorkAnswer>
    {
        public void Configure(EntityTypeBuilder<HomeWorkAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.HomeWorkResult)
              .WithMany(b => b.HomeWorkAnswers)
              .HasForeignKey(b => b.HomeWorkResultId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.HomeWorkQuestion)
              .WithMany(b => b.HomeWorkAnswers)
              .HasForeignKey(b => b.HomeWorkQuestionId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumAnswerStatus>());

            builder.HasIndex(c => new { c.HomeWorkQuestionId, c.HomeWorkResultId, c.IsDeleted });
        }
    }
}
