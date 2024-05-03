// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums;
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
               .IsRequired()
               .HasMaxLength(250)
               .IsUnicode(false) // Ensure Unicode is set to false
               .UseCollation("SQL_Latin1_General_CP1_CI_AI"); // Set the collation
        }
    }
}
