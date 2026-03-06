// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumDetailResultHistoryTypeConfiguration : IEntityTypeConfiguration<ClassForumDetailResultHistory>
    {
        public void Configure(EntityTypeBuilder<ClassForumDetailResultHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ClassForumDetailResult)
                   .WithMany(b => b.ClassForumDetailResultHistories)
                   .HasForeignKey(p => p.ClassForumDetailResultId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
