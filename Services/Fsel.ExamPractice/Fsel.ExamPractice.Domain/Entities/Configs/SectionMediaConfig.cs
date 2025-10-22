// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.Configs
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;

    public class SectionMediaConfig
    {
        [MaxLength(7000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? MediaPost { get; set; }

        [MaxLength(7000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Instruction { get; set; }

        public double? DisplayTime { get; set; }
        public double? ExecutionTime { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SubFilePath { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? VideoFilePath { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AudioPath { get; set; }

        public int TotalQuestion { get; set; }
        public int TargetWord { get; set; }

        [NotMapped]
        public TimeSpan DisplayTimeSpan
        {
            get { return TimeSpan.FromSeconds(DisplayTime ?? default); }
        }

        [NotMapped]
        public TimeSpan ExecutionTimeSpan
        {
            get { return TimeSpan.FromSeconds(ExecutionTime ?? default); }
        }
    }
}
