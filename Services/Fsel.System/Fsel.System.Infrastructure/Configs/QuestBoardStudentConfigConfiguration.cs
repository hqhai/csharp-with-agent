// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestBoardStudentConfigConfiguration : IEntityTypeConfiguration<QuestBoardStudent>
    {
        public void Configure(EntityTypeBuilder<QuestBoardStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumQuestBoardStatus>());

            builder.HasOne(a => a.QuestBoard)
                    .WithMany(b => b.QuestBoardStudents)
                    .HasForeignKey(b => b.QuestBoardId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
