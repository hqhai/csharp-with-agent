// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserCourseSettingEntityTypeConfiguration : IEntityTypeConfiguration<UserCourseSetting>
    {
        public void Configure(EntityTypeBuilder<UserCourseSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.User)
                 .WithMany(b => b.UserCourseSettings)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Type)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumUserCourseType>());

            builder.Property(e => e.CourseLevel)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumCourseLevel>());

            builder.HasIndex(x => x.UserId).IsUnique(false);
        }
    }
}
