// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class PlacementTestSectionResult : Entity
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
        /// Thời gian kết thúc làm bài
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public long EndTime { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumResultStatus Status { get; set; }

        public PlacementTestSection? PlacementTestSection { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid PlacementTestSectionId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
    }
}
