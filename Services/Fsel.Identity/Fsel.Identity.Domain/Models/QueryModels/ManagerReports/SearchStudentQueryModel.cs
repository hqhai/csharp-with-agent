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

        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public string? SchoolIdsStr { get; set; }

        public IList<Guid>? SchoolIds
        {
            get
            {
                return SchoolIdsStr.ToList<Guid>();
            }
        }

        public string? ListDistrict { get; set; }

        public IList<Guid>? DistrictIds
        {
            get
            {
                return ListDistrict.ToList<Guid>();
            }
        }

        public string? ListProvince { get; set; }

        public IList<Guid>? ProvinceIds
        {
            get
            {
                return ListProvince.ToList<Guid>();
            }
        }

        public string? ListStudentId { get; set; }

        public IList<Guid>? StudentIds
        {
            get
            {
                return ListStudentId.ToList<Guid>();
            }
        }

        public string? ListCourseType { get; set; }

        [JsonIgnore]
        public IList<EnumCourseType>? CourseTypes
        {
            get
            {
                return ListCourseType.ToList<EnumCourseType>();
            }
        }

        public bool? IsLearning { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
        public EnumCompletionStatus? Status { get; set; }
        public bool IsCheckDate { get; set; }
    }
}
