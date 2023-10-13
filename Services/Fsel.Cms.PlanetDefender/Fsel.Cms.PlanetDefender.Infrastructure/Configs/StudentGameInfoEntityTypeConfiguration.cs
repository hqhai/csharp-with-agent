// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentGameInfoEntityTypeConfiguration : IEntityTypeConfiguration<StudentGameInfo>
    {
        public void Configure(EntityTypeBuilder<StudentGameInfo> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Level)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumGameCourseLevel>());

            builder.Property(e => e.Gender)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumGender>());
        }
    }
}
