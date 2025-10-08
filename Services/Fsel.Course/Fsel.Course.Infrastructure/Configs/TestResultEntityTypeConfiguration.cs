// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestResultEntityTypeConfiguration : IEntityTypeConfiguration<TestResult>
    {
        public void Configure(EntityTypeBuilder<TestResult> builder)
        {
            builder.HasOne(a => a.Test)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.TestId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.TestGroupResult)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.TestGroupResultId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.StepFlow)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.StepFlowId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ActionFlow)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.ActionFlowId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
