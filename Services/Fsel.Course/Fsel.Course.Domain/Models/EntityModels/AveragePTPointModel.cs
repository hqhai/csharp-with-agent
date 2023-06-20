// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class AveragePTPointModel
    {
        public Guid ClassId { get; set; }
        public long AveragePTPoint { get; set; }
        public int MaxPTPoint { get; set; }
    }
}
