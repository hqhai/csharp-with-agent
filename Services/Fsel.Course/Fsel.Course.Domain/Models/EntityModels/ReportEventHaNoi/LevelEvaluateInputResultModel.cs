// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class LevelEvaluateInputResultModel
    {
        public int? AmountArchiveA1 { get; set; }

        public int? AmountArchiveA2 { get; set; }

        public int? AmountArchiveB1 { get; set; }

        public int? AmountArchiveB1Plus { get; set; }

        public int? AmountArchiveB2 { get; set; }

        public int? AmountArchiveC1 { get; set; }
    }
}
