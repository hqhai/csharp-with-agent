// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class FlowEntityTypeConfiguration : IEntityTypeConfiguration<Flow>
    {
        public void Configure(EntityTypeBuilder<Flow> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumFlowType>());

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumStatus>());

            builder.HasOne(a => a.Category)
                .WithMany(b => b.Flows)
                .HasForeignKey(b => b.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
