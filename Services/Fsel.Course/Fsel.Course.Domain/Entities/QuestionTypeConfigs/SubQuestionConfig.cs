// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs
{
    public class SubQuestionConfig
    {
        public string? Id { get; set; }
        public int Index { get; set; }
    }

    public interface IConfigRuby
    {
        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }
    }
}
