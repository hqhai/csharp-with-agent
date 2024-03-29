// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.Chatbots;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ChatbotConfigEntityTypeConfiguration : IEntityTypeConfiguration<ChatbotConfig>
    {
        public void Configure(EntityTypeBuilder<ChatbotConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumChatbotConfigStatus>());
        }
    }
}
