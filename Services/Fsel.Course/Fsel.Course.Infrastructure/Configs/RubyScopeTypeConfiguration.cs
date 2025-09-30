// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using EntityRubyDocument = Fsel.Course.Domain.Entities.RubyScope;

    public class RubyScopeTypeConfiguration : IEntityTypeConfiguration<EntityRubyDocument>
    {
        public void Configure(EntityTypeBuilder<EntityRubyDocument> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.ObjectType)
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumObjectType>());
        }
    }
}
