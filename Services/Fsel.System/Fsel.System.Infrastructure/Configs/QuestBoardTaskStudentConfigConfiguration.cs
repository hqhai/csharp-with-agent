// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestBoardTaskStudentConfigConfiguration : IEntityTypeConfiguration<QuestBoardTaskStudent>
    {
        public void Configure(EntityTypeBuilder<QuestBoardTaskStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumQuestBoardStatus>());

            builder.HasOne(a => a.QuestBoardTask)
                    .WithMany(b => b.QuestBoardTaskStudents)
                    .HasForeignKey(b => b.QuestBoardTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
