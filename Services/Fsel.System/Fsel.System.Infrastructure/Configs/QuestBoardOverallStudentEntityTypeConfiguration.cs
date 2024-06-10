// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.QuestBoards;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestBoardOverallStudentEntityTypeConfiguration : IEntityTypeConfiguration<QuestBoardOverallStudent>
    {
        public void Configure(EntityTypeBuilder<QuestBoardOverallStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumQuestBoardOverallStudentStatus>());

            builder.HasOne(a => a.QuestBoardOverall)
                    .WithMany(b => b.QuestBoardOverallStudents)
                    .HasForeignKey(b => b.QuestBoardOverallId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
