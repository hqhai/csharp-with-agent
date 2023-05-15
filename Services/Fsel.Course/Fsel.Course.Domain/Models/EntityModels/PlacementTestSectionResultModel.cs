// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class PlacementTestSectionResultModel : BaseModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public long EndTime { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid PlacementTestSectionId { get; set; }
        public Guid StudentId { get; set; }
    }
}
