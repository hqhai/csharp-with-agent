// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class ClassForumSearchModel
    {
        public string? PromptName { get; set; }

        public long? TaggetWordLimit { get; set; }

        public double? TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public IList<ClassForumFileModel>? ClassForumFiles { get; set; }

        public string? Content { get; set; }

        public int? WordCount { get; set; }

        public int? TimeCount { get; set; }

        public string? GradingAlFeedback { get; set; }

        public double CorrectCount { get; set; }

        public double Score { get; set; }

        public double CorrectTotal
        {
            get
            {
                return CorrectCount + Score;
            }
        }

        public IList<ClassForumResultFileModel>? ClassForumResultFiles { get; set; }
    }
}
