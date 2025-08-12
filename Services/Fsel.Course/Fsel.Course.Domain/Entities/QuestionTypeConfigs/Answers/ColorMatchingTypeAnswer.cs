// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class ColorMatchingTypeAnswer
    {
        public IList<ColorMatchingTypeAnswers> Answers { get; set; } = new List<ColorMatchingTypeAnswers>();
    }

    public class ColorMatchingTypeAnswers
    {
        public string? OptionId { get; set; }
        public bool IsSelected { get; set; }
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