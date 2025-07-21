// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class GapFillAnswer
    {
        public IList<GapFillAnswers> Answers { get; set; } = new List<GapFillAnswers>();
    }

    public class GapFillAnswers
    {
        private IList<bool>? _isFirstSubmits;
        public long Id { get; set; }
        public IList<string>? Answer { get; set; }
        public IList<bool?>? IsExacts { get; set; }

        public IList<EnumCorrectStatus>? Statuses
        {
            get
            {
                return IsExacts?.Select(x =>
                {
                    return x.HasValue ? x.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail : EnumCorrectStatus.Process;
                }).ToList();
            }
        }

        public IList<bool>? IsFirstSubmits
        {
            get
            {
                if (_isFirstSubmits == null || _isFirstSubmits.Count == 0)
                {
                    return IsExacts?.Select(x => true).ToList();
                }
                return _isFirstSubmits;
            }
            set
            {
                _isFirstSubmits = value;
            }
        }
    }
}
