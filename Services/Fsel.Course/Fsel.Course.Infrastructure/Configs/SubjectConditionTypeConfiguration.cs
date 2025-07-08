// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SubjectConditionTypeConfiguration : IEntityTypeConfiguration<SubjectCondition>
    {
        public void Configure(EntityTypeBuilder<SubjectCondition> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumConditionType>());

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.SubjectConditions)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
