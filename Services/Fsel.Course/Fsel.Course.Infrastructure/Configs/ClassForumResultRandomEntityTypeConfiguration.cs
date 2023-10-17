// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumResultRandomEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumResultRandom>
    {
        public void Configure(EntityTypeBuilder<ClassForumResultRandom> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ClassForum)
                .WithMany(b=>b.ClassForumResultRandoms)
                .HasForeignKey(p => p.ClassForumId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(a => a.ClassForumResult)
                 .WithMany(b => b.ClassForumResultRandoms)
                 .HasForeignKey(p => p.ClassForumResultId)
                 .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
