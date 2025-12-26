// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SkillLevelEntityTypeConfiguration : IEntityTypeConfiguration<SkillLevel>
    {
        public void Configure(EntityTypeBuilder<SkillLevel> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.SkillLevels)
                   .HasForeignKey(p => p.LevelId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Skill)
                   .WithMany(b => b.SkillLevels)
                   .HasForeignKey(p => p.SkillId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
