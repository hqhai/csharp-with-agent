// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchStudentReportQueryModel : SearchStudentSchoolQueryModel
    {
        public DateTime? StartDate { get; set; }
        public bool IsSearchReport { get; set; }
        public override bool? IsLearning { get; set; } = true;
    }
}
