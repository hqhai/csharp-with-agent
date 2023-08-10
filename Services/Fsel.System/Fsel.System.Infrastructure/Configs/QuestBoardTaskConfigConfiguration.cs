// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestBoardTaskConfigConfiguration : IEntityTypeConfiguration<QuestBoardTask>
    {
        public void Configure(EntityTypeBuilder<QuestBoardTask> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}
