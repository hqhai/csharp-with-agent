// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class UnitDoneLearningProgressAcademicModel
    {
        public DateTime? CompletedDate { get; set; }

        public int? TotalUnitDone { get; set; }
    }
}
