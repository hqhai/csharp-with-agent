// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class QuestionFormEntityTypeConfiguration : IEntityTypeConfiguration<QuestionForm>
    {
        public void Configure(EntityTypeBuilder<QuestionForm> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumQuestionType>());
        }
    }
}
