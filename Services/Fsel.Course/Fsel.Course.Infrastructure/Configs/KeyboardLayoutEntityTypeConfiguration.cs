// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class KeyboardLayoutEntityTypeConfiguration : IEntityTypeConfiguration<KeyboardLayout>
    {
        public void Configure(EntityTypeBuilder<KeyboardLayout> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}
