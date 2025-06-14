// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StepFlowEntityTypeConfiguration : IEntityTypeConfiguration<StepFlow>
    {
        public void Configure(EntityTypeBuilder<StepFlow> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                     .HasMaxLength(20)
                     .HasConversion(
                         v => v.ToString(),
                         v => v.EnumParse<EnumStepFlowType>());

            builder.HasOne(a => a.Flow)
                   .WithMany(b => b.StepFlows)
                   .HasForeignKey(p => p.FlowId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.StepFlows)
                   .HasForeignKey(p => p.LevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ParentFlowStep)
                    .WithMany(b => b.StepFlows)
                    .HasForeignKey(p => p.ParentId)
                    .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
