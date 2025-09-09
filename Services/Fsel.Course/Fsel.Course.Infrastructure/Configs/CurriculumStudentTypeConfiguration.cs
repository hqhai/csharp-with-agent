// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CurriculumStudentTypeConfiguration : IEntityTypeConfiguration<CurriculumStudent>
    {
        public void Configure(EntityTypeBuilder<CurriculumStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(e => e.Curriculum)
                .WithMany(c => c.CurriculumStudent)
                .HasForeignKey(e => e.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.CurriculumStudentStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCurriculumStudent>());
        }
    }
}
