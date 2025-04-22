// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class TotalLearningModel
    {
        public int? TotalStudent { get; set; }
        public int? TotalStudentJoin { get; set; }
        public int? TotalJoinAcademic { get; set; }
        public int? TotalJoinIelts { get; set; }
        public double? AverageDoneQuestion { get; set; }
    }
}
