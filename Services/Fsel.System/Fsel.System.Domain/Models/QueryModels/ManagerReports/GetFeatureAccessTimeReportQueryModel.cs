// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels.ManagerReports
{
    public class GetFeatureAccessTimeReportQueryModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
    }
}
