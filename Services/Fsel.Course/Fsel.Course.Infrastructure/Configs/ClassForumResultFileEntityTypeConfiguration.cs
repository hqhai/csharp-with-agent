// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumResultFileEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumResultFile>
    {
        public void Configure(EntityTypeBuilder<ClassForumResultFile> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ClassForumResult)
                 .WithMany(b => b.ClassForumResultFiles)
                 .HasForeignKey(p => p.ClassForumResultId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
