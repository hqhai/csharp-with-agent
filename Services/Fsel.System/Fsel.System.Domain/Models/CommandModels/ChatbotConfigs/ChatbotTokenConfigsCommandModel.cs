// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatbotConfigs
{
    public class ChatbotTokenConfigsCommandModel
    {
        public int ReadingToken { get; set; }
        public int ListeningToken { get; set; }
        public int WritingToken { get; set; }
        public int SpeakingToken { get; set; }
        public int VocabularyToken { get; set; }
        public int GrammarToken { get; set; }
        public Guid? Id { get; set; }
    }
}
