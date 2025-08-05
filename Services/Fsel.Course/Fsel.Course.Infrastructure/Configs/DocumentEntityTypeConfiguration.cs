// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DocumentEntityTypeConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.VersionStatus)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumVersionStatus>());
        }
    }
}
