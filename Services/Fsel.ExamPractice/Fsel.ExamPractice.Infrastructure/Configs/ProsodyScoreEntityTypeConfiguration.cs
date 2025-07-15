// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.ExamPractice.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ProsodyScoreEntityTypeConfiguration : IEntityTypeConfiguration<ProsodyScore>
    {
        public void Configure(EntityTypeBuilder<ProsodyScore> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}
