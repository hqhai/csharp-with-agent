// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchStudentReportQueryModel : SearchStudentSchoolQueryModel
    {
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
            LevelIdStr = source.LevelIdStr;
            SubjectId = source.SubjectId;

            StartDate = source.StartDate;
            EndDate = source.EndDate;

            ProgramId = source.ProgramId;
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
