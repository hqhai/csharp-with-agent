// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class ConfigAnswerV1 : IConfigRuby
    {
        public Guid? Id { get; set; } = Guid.NewGuid();
        public string? Content { get; set; }
        public string? Key { get; set; }
        public bool? IsCorrect { get; set; }
        public Guid? RowId { get; set; }
        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }
    }

    public class ConfigQuestionV1 : ConfigAnswerV1
    {
        public IList<ConfigAnswerV1> Answers { get; set; } = new List<ConfigAnswerV1>();
    }

    public class ConfigContentQuestionV1 : ConfigAnswerV1
    {
        public IList<ConfigAnswerV1> Contents { get; set; } = new List<ConfigAnswerV1>();
    }
}
