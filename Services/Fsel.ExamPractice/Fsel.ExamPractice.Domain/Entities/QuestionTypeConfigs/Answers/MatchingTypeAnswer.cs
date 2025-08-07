// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class MatchingTypeAnswer
    {
        public IList<MatchingTypeAnswers> Answers { get; set; } = new List<MatchingTypeAnswers>();
    }

    public class MatchingTypeAnswers
    {
        public long? FromId { get; set; }
        public long? ToId { get; set; }
        public bool? IsExact { get; set; }

        public EnumCorrectStatus Status
        {
            get
            {
                return IsExact.HasValue ? IsExact.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail : EnumCorrectStatus.Process;
            }
        }

        public bool IsFirstSubmit { get; set; } = true;
    }
}
