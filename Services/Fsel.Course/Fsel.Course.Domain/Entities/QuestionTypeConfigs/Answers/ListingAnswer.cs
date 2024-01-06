// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using Fsel.Shared.Enums;

    public class ListingAnswer
    {
        public IList<string>? Answers { get; set; }
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
