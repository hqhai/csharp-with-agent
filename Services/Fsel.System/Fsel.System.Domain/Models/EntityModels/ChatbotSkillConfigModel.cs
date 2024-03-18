// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class ChatbotSkillConfigModel
    {
        public IList<SkillConfig>? SkillConfigs { get; set; }

        public ChatbotTokenConfigs? ChatbotTokenConfigs { get; set; }
        //public IList<ChatbotSkillVocabularyModel>? VocabularyModels { get; set; }
        //public ChatbotSkillGrammarModel? GrammarModel { get; set; }
        //public ChatbotSkillReadingListeningModel? ReadingModel { get; set; }
        //public ChatbotSkillReadingListeningModel? ListeningModel { get; set; }
        //public ChatbotSkillWritingSpeakingModel? WritingModel { get; set; }
        //public ChatbotSkillWritingSpeakingModel? SpeakingModel { get; set; }
    }


    #region OldModel
    public class ChatbotSkillVocabularyModel
    {
        public string? Name { get; set; }
        public IList<string>? Items { get; set; }
    }

    public class ChatbotSkillGrammarModel
    {
        public string? Name { get; set; }

        public IList<ChatBotGrammarItemModel>? Items { get; set; }

    }

    public class ChatBotGrammarItemModel
    {
        public IList<string>? Items { get; set; }

        public EnumGrammarPromptType Type { get; set; }
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
    #endregion


    public class SkillConfig
    {
        public EnumSkillAiConfigType? SkillConfigType { get; set; }
        public IList<SkillBlockItem>? BlockItems { get; set; }
    }

    public class SkillBlockItem
    {
        public string? Name { get; set; }
        public IList<ItemSkillContent>? ItemSkillContent { get; set; }
    }

    public class ItemSkillContent
    {
        public EnumGrammarPromptType? ContentType { get; set; }

        public string? Content { get; set; }

    }

    public class ChatbotTokenConfigs
    {
        public int ReadingToken { get; set; }
        public int ListeningToken { get; set; }
        public int Writing { get; set; }
        public int Speaking { get; set; }
        public int Vocabulary { get; set; }
        public int Grammar { get; set; }
    }
}
