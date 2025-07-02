// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchReportLearningProgressQueryModel : SearchStudentSchoolQueryModel
    {
        [JsonIgnore]
        public IList<EnumCourseType>? CourseTypes
        {
            get
            {
                return ListCourseType.ToList<EnumCourseType>();
            }
        }
    }
}
