// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models
{
    using Fsel.Shared.Enums;

    public class QuestionResultQueueModel
    {
        public Guid TResultId { get; set; }

        public Guid QuestionId { get; set; }

        public EnumQuestionResultType Type { get; set; }

        public int CountFail { get; set; }

        public int CountStrokes { get; set; }
    }
}
