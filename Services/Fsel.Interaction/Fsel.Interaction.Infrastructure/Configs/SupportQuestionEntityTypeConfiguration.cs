// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using System;
    using Fsel.Interaction.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SupportQuestionEntityTypeConfiguration : IEntityTypeConfiguration<SupportQuestion>
    {
        public void Configure(EntityTypeBuilder<SupportQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.SupportCategory)
                .WithMany(b => b.SupportQuestions)
                .HasForeignKey(b => b.SupportCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
