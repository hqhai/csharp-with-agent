// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ActionFlowEntityTypeConfiguration : IEntityTypeConfiguration<ActionFlow>
    {
        public void Configure(EntityTypeBuilder<ActionFlow> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.FromStepFlow)
                   .WithMany(b => b.ChildActionFlows)
                   .HasForeignKey(p => p.FromStepFlowId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ToStepFlow)
                    .WithMany(b => b.ParentActionFlows)
                    .HasForeignKey(p => p.ToStepFlowId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
