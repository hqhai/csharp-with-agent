// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    public class TracingAnswer
    {
        public int CountFail { get; set; }
        public int CountStrokes { get; set; }
        public bool IsExact { get; set; }
        public bool IsFirstSubmit { get; set; } = true;
    }
}
