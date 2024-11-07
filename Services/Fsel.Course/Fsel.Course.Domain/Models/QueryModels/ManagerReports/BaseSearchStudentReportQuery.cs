// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Helpers;

    public class BaseSearchStudentReportQuery : BaseQueryModel
    {
        public string? ListSchool { get; set; }

        public IList<Guid>? SchoolIds
        {
            get
            {
                return ListSchool.ToList<Guid>();
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

        public string? SchoolGrade { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
