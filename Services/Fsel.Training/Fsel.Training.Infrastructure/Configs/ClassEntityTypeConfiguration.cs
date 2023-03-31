// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Training.Doman.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassEntityTypeConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassType>());
        }
    }
}
