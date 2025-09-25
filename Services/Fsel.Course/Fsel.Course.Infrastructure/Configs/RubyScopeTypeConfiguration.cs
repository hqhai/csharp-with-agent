// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using EntityRubyDocument = Fsel.Course.Domain.Entities.RubyScope;

    public class RubyScopeTypeConfiguration : IEntityTypeConfiguration<EntityRubyDocument>
    {
        public void Configure(EntityTypeBuilder<EntityRubyDocument> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}
