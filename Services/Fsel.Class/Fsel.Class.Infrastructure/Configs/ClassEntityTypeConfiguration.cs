// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Infrastructure.Configs
{
    using Fsel.Class.Doman.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Classes = Fsel.Class.Doman.Entities.Class;

    public class ClassEntityTypeConfiguration : IEntityTypeConfiguration<Classes>
    {
        public void Configure(EntityTypeBuilder<Classes> builder)
        {
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassType>());
        }
    }
}
