// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumDetailResultTypeConfiguration : IEntityTypeConfiguration<ClassForumDetailResult>
    {
        public void Configure(EntityTypeBuilder<ClassForumDetailResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ClassForumResult)
                 .WithMany(b => b.ClassForumDetailResults)
                 .HasForeignKey(p => p.ClassForumResultId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
