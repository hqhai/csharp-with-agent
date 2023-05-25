// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumResultEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumResult>
    {
        public void Configure(EntityTypeBuilder<ClassForumResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumStatus>());

            builder.HasOne(a => a.ClassForum)
                .WithOne(b => b.ClassForumResult)
                .HasForeignKey<ClassForumResult>(p => p.ClassForumId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
