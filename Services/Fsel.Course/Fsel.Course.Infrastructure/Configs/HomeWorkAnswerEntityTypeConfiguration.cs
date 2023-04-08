// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
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
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.HomeWorkQuestion)
              .WithMany(b => b.HomeWorkAnswers)
              .HasForeignKey(b => b.HomeWorkQuestionId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
