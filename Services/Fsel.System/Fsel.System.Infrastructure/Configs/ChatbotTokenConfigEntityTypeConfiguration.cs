// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities.Chatbots;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ChatbotTokenConfigEntityTypeConfiguration : IEntityTypeConfiguration<ChatbotTokenConfigs>
    {
        public void Configure(EntityTypeBuilder<ChatbotTokenConfigs> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ChatbotConfig)
                   .WithOne(b => b.ChatbotTokenConfigs)
                   .HasForeignKey<ChatbotTokenConfigs>(b => b.ChatbotConfigId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
