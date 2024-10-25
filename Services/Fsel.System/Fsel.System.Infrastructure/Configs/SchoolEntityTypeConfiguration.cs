// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SchoolEntityTypeConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.EducationLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumEducationLevel>());

            builder.Property(e => e.SchoolType)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumSchoolType>());

            builder.HasOne(a => a.Location)
                   .WithMany(b => b.Schools)
                   .HasForeignKey(b => b.LocationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.Property(l => l.Name)
               .UseCollation(CollationSetting.SQLLatin1GeneralCP1CIAI); // Set the collation
        }
    }
}
