// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class FinalTest : Entity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Thời gian hiện làm bài
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double ExecutionTime { get; set; }

        /// <summary>
        /// Loại FinalTest
        /// </summary>
        public EnumFinalTestLevel FinalTestLevel { get; set; }

        public ICollection<CourseUnitMockTest>? CourseUnitMockTests { get; set; }
        public ICollection<FinalTestSection> FinalTestSections { get; set; } = new List<FinalTestSection>();
        public ICollection<FinalTestResult> FinalTestResults { get; set; } = new List<FinalTestResult>();
    }
}
