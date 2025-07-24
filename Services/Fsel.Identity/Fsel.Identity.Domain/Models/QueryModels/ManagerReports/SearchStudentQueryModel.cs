// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.ManagerReports
{
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SearchStudentQueryModel : BaseQueryModel
    {
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }
        public string? ListDistrict { get; set; }
        public string? SchoolIdsStr { get; set; }
        public string? ListProvince { get; set; }
        public string? ListStudentId { get; set; }
        public string? ListCourseType { get; set; }
        public string? ListCourseLevel { get; set; }
        public string? ListLearningStatus { get; set; }

        public IList<EnumCourseLevel>? CourseLevels
        {
            get
            {
                return ListCourseLevel.ToList<EnumCourseLevel>();
            }
        }

        public IList<string>? SchoolClasses
        {
            get
            {
                return ListSchoolClass.ToList<string>();
            }
        }

        public IList<string>? SchoolGrades
        {
            get
            {
                return ListSchoolGrade.ToList<string>();
            }
        }

        public IList<Guid>? SchoolIds
        {
            get
            {
                return SchoolIdsStr.ToList<Guid>();
            }
        }

        public IList<Guid>? DistrictIds
        {
            get
            {
                return ListDistrict.ToList<Guid>();
            }
        }

        public IList<Guid>? ProvinceIds
        {
            get
            {
                return ListProvince.ToList<Guid>();
            }
        }

        public IList<Guid>? StudentIds
        {
            get
            {
                return ListStudentId.ToList<Guid>();
            }
        }

        [JsonIgnore]
        public IList<EnumCourseType>? CourseTypes
        {
            get
            {
                return ListCourseType.ToList<EnumCourseType>();
            }
        }

        [JsonIgnore]
        public IList<EnumLearningStatus>? LearningStatuses
        {
            get
            {
                return ListLearningStatus.ToList<EnumLearningStatus>();
            }
        }

        public bool? IsLearning { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public bool IsCheckDate { get; set; }
    }
}
