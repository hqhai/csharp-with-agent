// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class BaseSearchStudentReportQuery : BaseQueryModel
    {
        public string? ListSchool { get; set; }

        public IList<Guid> SchoolIds
        {
            get
            {
                return ListSchool.ToList<Guid>() ?? new List<Guid>();
            }
        }

        public string? ListDistrict { get; set; }

        public IList<Guid> DistrictIds
        {
            get
            {
                return ListDistrict.ToList<Guid>() ?? new List<Guid>();
            }
        }

        public string? ListProvince { get; set; }

        public IList<Guid> ProvinceIds
        {
            get
            {
                return ListProvince.ToList<Guid>() ?? new List<Guid>();
            }
        }

        public string? SchoolClass { get; set; }
        public string? SchoolGrade { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
