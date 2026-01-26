// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.Chatbots;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ChatBotEntityTypeConfiguration : IEntityTypeConfiguration<ChatBot>
    {
        public void Configure(EntityTypeBuilder<ChatBot> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Skill)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseSkill>());

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumChatBotStatus>());
        }
    }
}
