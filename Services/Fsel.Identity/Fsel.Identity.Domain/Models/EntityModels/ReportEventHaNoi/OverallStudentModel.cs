// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class OverallStudentModel
    {
        public int? TotalStudents { get; set; }
        public int? ActiveStudentsToday { get; set; }
    }
}
