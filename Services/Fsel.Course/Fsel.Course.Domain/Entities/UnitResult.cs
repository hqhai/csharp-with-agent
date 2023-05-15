// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Common.Helpers;
    using System.ComponentModel.DataAnnotations.Schema;

    public class UnitResult : Entity
    {
        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>
        private double _percent;

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double Percent
        {
            get
            {
                return CorrectTotal > 0 ? ((double)CorrectCount / CorrectTotal * 100) : _percent;
            }
            set { _percent = CorrectTotal > 0 ? ((double)CorrectCount / CorrectTotal * 100) : value; }
        }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumResultStatus Status { get; set; }

        public string? Score { get; set; }

        [NotMapped]
        public object? SkillScore
        {
            get { return ConvertHelper.Deserialize<object>(Score); }
            set { Score = ConvertHelper.Serialize(value); }
        }

        /// <summary>
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotal { get; set; }

        public Course? Course { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CourseId { get; set; }

        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UnitId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }
    }
}
