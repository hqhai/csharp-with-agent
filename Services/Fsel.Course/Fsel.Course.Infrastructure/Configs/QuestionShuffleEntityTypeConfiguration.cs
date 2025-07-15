// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestionShuffleEntityTypeConfiguration : IEntityTypeConfiguration<QuestionShuffle>
    {
        public void Configure(EntityTypeBuilder<QuestionShuffle> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Question)
                   .WithMany(b => b.QuestionShuffles)
                   .HasForeignKey(b => b.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.StudentId, c.QuestionId }).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
