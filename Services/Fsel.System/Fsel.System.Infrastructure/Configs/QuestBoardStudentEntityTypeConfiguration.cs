// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.QuestBoards;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestBoardStudentEntityTypeConfiguration : IEntityTypeConfiguration<QuestBoardStudent>
    {
        public void Configure(EntityTypeBuilder<QuestBoardStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumQuestBoardStudentStatus>());

            builder.HasOne(a => a.QuestBoard)
                    .WithMany(b => b.QuestBoardStudents)
                    .HasForeignKey(b => b.QuestBoardId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.QuestBoardOverallStudent)
                    .WithMany(b => b.QuestBoardStudents)
                    .HasForeignKey(b => b.QuestBoardOverallStudentId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.IsDeleted, c.QuestBoardId, c.StudentId, c.CreatedDate });
        }
    }
}
