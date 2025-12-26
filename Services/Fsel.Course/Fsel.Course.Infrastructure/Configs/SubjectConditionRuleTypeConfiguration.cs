// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class SubjectConditionRuleTypeConfiguration : IEntityTypeConfiguration<SubjectConditionRule>
    {
        public void Configure(EntityTypeBuilder<SubjectConditionRule> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(x => x.SubjectCondition)
                   .WithMany(x => x.SubjectConditionRules)
                   .HasForeignKey(x => x.SubjectConditionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
