// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.QueryModels.StudentDashboard
{
    using System;
    using System.Collections.Generic;

    public class BaseStudentDashboardQueryModel
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public IList<Guid>? SubjectIds { get; set; }

        public IList<Guid>? ProvinceIds { get; set; }

        public IList<Guid>? DistrictIds { get; set; }

        public IList<Guid>? SchoolIds { get; set; }
    }
}
