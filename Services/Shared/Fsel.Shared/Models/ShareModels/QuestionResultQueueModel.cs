// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class QuestionResultQueueModel
    {
        public Guid TResultId { get; set; }

        public Guid QuestionId { get; set; }

        public EnumQuestionResultType Type { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public object? Config { get; set; }
    }
}
