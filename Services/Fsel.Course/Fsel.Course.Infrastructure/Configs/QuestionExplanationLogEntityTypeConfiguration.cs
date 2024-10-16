// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestionExplanationLogEntityTypeConfiguration : IEntityTypeConfiguration<QuestionExplanationLog>
    {
        public void Configure(EntityTypeBuilder<QuestionExplanationLog> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Question)
                  .WithMany(b => b.QuestionExplanationLogs)
                  .HasForeignKey(b => b.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
