// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class DistrictInfoModel
    {
        public Guid? DistrictId { get; set; }

        public string? DistrictName { get; set; }
    }
}
