// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using System;
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassLiveWorkFlowPlanEntityTypeConfiguration : IEntityTypeConfiguration<ClassLiveWorkFlowPlan>
    {
        public void Configure(EntityTypeBuilder<ClassLiveWorkFlowPlan> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ClassLiveWorkFlow)
               .WithMany(b => b.ClassLiveWorkFlowPlans)
               .HasForeignKey(b => b.ClassLiveWorkFlowId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
