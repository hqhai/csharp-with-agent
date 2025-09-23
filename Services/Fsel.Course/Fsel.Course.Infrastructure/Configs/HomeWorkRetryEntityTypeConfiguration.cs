// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HomeWorkRetryEntityTypeConfiguration : IEntityTypeConfiguration<HomeWorkRetry>
    {
        public void Configure(EntityTypeBuilder<HomeWorkRetry> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.HomeWork)
                   .WithMany(b => b.HomeWorkRetrys)
                   .HasForeignKey(b => b.HomeWorkId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Curriculum)
                   .WithMany(b => b.HomeWorkRetrys)
                   .HasForeignKey(b => b.CurriculumId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
