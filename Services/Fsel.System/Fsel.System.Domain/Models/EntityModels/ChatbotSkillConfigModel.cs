// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    public class ChatbotSkillConfigModel
    {
        public IList<ChatbotSkillVocabularyModel>? VocabularyModels { get; set; }
        public ChatbotSkillGrammarModel? GrammarModel { get; set; }
        public ChatbotSkillReadingListeningModel? ReadingModel { get; set; }
        public ChatbotSkillReadingListeningModel? ListeningModel { get; set; }
        public ChatbotSkillWritingSpeakingModel? WritingModel { get; set; }
        public ChatbotSkillWritingSpeakingModel? SpeakingModel { get; set; }
    }

    public class ChatbotSkillVocabularyModel
    {
        public string? Name { get; set; }
        public IList<string>? Items { get; set; }
    }

    public class ChatbotSkillGrammarModel
    {
        public IList<string>? Items { get; set; }
    }

    public class ChatbotSkillReadingListeningModel
    {
        public string? Name { get; set; }
        public ChatbotSkillVocabularyModel? Vocabs { get; set; }
        public ChatbotSkillGrammarModel? Grammars { get; set; }
    }

    public class ChatbotSkillWritingSpeakingModel
    {
        public ChatbotSkillGrammarModel? Grammars { get; set; }
    }
}
