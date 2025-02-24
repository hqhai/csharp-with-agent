// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]

    public class NumberStudentLearnOnSystemModel
    {
        public DateTime? ActiveDate { get; set; }
        public int? ActiveStudentCount { get; set; }
    }
}
