// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class NumberStudentLearnOnSystemModel
    {
        public string? ActiveDate { get; set; }
        public int? ActiveStudentCount { get; set; }
    }
}
