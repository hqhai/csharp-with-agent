// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SearchStudentQueryModel : BaseQueryModel
    {
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

        public bool? IsLearning { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
        public EnumCompletionStatus? Status { get; set; }
        public bool IsCheckDate { get; set; }
    }
}
