// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class SchoolInfoModel
    {
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? Level { get; set; }
    }
}
