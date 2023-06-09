// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HomeWorkQuestionEntityTypeConfiguration : IEntityTypeConfiguration<HomeWorkQuestion>
    {
        public void Configure(EntityTypeBuilder<HomeWorkQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.HomeWork)
                .WithMany(b => b.HomeWorkQuestions)
                .HasForeignKey(a => a.HomeWorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                .WithMany(b => b.HomeWorkQuestions)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
