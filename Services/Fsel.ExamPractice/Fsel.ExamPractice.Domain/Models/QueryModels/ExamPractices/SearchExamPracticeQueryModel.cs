// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.QueryModels.ExamPractices
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Helpers;

    public class SearchExamPracticeQueryModel : BaseQueryModel
    {
        public EnumExamPracticeType Type { get; set; }
        public string? SubTypeStr { get; set; }

        public IList<EnumExamPracticeSubType>? SubTypes
        {
            get
            {
                return SubTypeStr.ToList<EnumExamPracticeSubType>();
            }
        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
