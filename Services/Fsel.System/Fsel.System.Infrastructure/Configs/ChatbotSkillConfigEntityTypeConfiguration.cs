// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.Chatbots;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ChatbotSkillConfigEntityTypeConfiguration : IEntityTypeConfiguration<ChatbotSkillConfig>
    {
        public void Configure(EntityTypeBuilder<ChatbotSkillConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Skill)
                   .HasMaxLength(100)
                   .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumCourseSkill>());

            builder.Property(e => e.ChatbotLayout)
                    .HasMaxLength(100)
                    .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumChatbotLayout>());

            builder.HasOne(a => a.ChatbotConfig)
                   .WithMany(b => b.ChatbotSkillConfigs)
                   .HasForeignKey(b => b.ChatbotConfigId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
