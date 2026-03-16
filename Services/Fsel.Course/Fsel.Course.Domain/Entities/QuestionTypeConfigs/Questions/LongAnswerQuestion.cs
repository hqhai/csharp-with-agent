// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    public class LongAnswerQuestion : IConfigRuby
    {
        public string? Name { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }

        public int? Row { get; set; }
        public int? Column { get; set; }
        public Guid? AICriteriaConfigId { get; set; }
    }
}
