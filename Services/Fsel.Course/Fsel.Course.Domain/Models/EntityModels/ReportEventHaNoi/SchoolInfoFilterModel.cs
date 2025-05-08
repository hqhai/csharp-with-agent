// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class SchoolInfoFilterModel
    {
        public Guid? SchoolId { get; set; }

        public string? SchoolName { get; set; }
    }
}
