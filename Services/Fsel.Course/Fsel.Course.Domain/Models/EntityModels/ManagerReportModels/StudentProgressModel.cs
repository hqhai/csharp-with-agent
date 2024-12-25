// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    public class StudentProgressModel
    {
        public Guid StudentId { get; set; }
        public int CountProgress { get; set; }
        public int TotalProgress { get; set; }
    }
}
