// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Chatbots
{
    using Fsel.Core.Entities;

    public class ChatbotTokenConfigs : Entity
    {
        public int ReadingToken { get; set; }
        public int ListeningToken { get; set; }
        public int Writing { get; set; }
        public int Speaking { get; set; }
        public int Vocabulary { get; set; }
        public int Grammar { get; set; }
        public Guid ChatbotConfigId { get; set; }
        public ChatbotConfig? ChatbotConfig { get; set; }
    }
}
