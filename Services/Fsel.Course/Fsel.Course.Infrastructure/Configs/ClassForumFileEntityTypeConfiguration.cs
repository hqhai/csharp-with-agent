// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumFileEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumFile>
    {
        public void Configure(EntityTypeBuilder<ClassForumFile> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ClassForum)
                 .WithMany(b => b.ClassForumFiles)
                 .HasForeignKey(p => p.ClassForumId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
