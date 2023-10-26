// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Helpers;

    public class BaseResult : Entity
    {
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

        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>
        private double _percent;

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public virtual double Percent
        {
            get
            {
                return CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount, CorrectTotal) : _percent;
            }
            set { _percent = CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount, CorrectTotal) : value; }
        }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumResultStatus Status { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }
    }
}
