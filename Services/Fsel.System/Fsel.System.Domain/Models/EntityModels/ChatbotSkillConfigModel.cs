// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ChatbotSkillConfigModel : BaseModel
    {
        public IList<SkillConfigModel>? Configs { get; set; }
        public string? AiConfig { get; set; }
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


    public class SkillConfigModel
    {
        public EnumCourseSkill? SkillConfigType { get; set; }
        public IList<SkillBlockItemModel>? BlockItems { get; set; }
    }

    public class SkillBlockItemModel
    {
        public string? Name { get; set; }
        public IList<ItemSkillContentModel>? ItemSkillContent { get; set; }
    }

    public class ItemSkillContentModel
    {
        public EnumGrammarPromptType? ContentType { get; set; }

        public string? Content { get; set; }

    }
}
