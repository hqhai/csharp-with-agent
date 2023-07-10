// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using System;
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeacherFreeDateEntityTypeConfiguration : IEntityTypeConfiguration<TeacherFreeDate>
    {
        public void Configure(EntityTypeBuilder<TeacherFreeDate> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}
