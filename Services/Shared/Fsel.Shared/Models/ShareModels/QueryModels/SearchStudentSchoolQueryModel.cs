// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.QueryModels
{
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SearchStudentSchoolQueryModel : BaseQueryModel
    {
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }
        public string? ListSchool { get; set; }
        public string? ListDistrict { get; set; }
        public string? ListProvince { get; set; }
        public string? ListCompletionStatus { get; set; }
        public string? ListLearningStatus { get; set; }
        public string? ListOverallScore { get; set; }
        public virtual bool? IsLearning { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }
        public string? LevelIdStr { get; set; }

        public bool? LearningStatus { get; set; }
        public IList<Guid>? StudentIds { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsSearchReport { get; set; }
        public EnumManagerReportType ManagerReportType { get; set; }

        [JsonIgnore]
        public IList<EnumOverallScore>? OverallScores => ListOverallScore.ToList<EnumOverallScore>();

        [JsonIgnore]
        public IList<Guid>? LevelIds => LevelIdStr.ToList<Guid>();

        [JsonIgnore]
        public IList<EnumCompletionStatus>? CompletionStatuses => ListCompletionStatus.ToList<EnumCompletionStatus>();

        [JsonIgnore]
        public IList<EnumLearningStatus>? LearningStatuses => ListLearningStatus.ToList<EnumLearningStatus>();

        /// <summary>
        /// Copy toàn bộ filter/paging từ source
        /// </summary>
        protected void CopyFrom(SearchStudentSchoolQueryModel source)
        {
            Keyword = source.Keyword;

            ListDistrict = source.ListDistrict;
            ListProvince = source.ListProvince;
            ListSchool = source.ListSchool;
            ListSchoolClass = source.ListSchoolClass;
            ListSchoolGrade = source.ListSchoolGrade;

            ListCompletionStatus = source.ListCompletionStatus;
            ListLearningStatus = source.ListLearningStatus;
            ListOverallScore = source.ListOverallScore;

            IsLearning = source.IsLearning;
            LearningStatus = source.LearningStatus;

            StartDate = source.StartDate;
            EndDate = source.EndDate;

            ProgramId = source.ProgramId;
            LevelIdStr = source.LevelIdStr;
            SubjectId = source.SubjectId;
            StudentIds = source.StudentIds;

            Page = source.Page;
            PageSize = source.PageSize;
            SortBy = source.SortBy;
            Filters = source.Filters;
            IncludePaths = source.IncludePaths;

            IsSearchReport = source.IsSearchReport;
            ManagerReportType = source.ManagerReportType;
        }
    }
}
