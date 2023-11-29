// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class FinalTest : Entity
    {
        private double _executionTime;

        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Thời gian hiện làm bài
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double ExecutionTime
        {
            get
            {
                var sectionGroups = FinalTestSections.Select(x => x.SectionGroup).ToList();
                var executionTime = sectionGroups.Sum(x => x!.ExecutionTime);
                return executionTime > 0 ? executionTime : _executionTime;
            }
            set
            {
                _executionTime = value;
            }
        }

        /// <summary>
        /// Loại FinalTest
        /// </summary>
        public EnumFinalTestLevel FinalTestLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<FinalTestSection> FinalTestSections { get; set; } = new List<FinalTestSection>();
        public ICollection<FinalTestResult> FinalTestResults { get; set; } = new List<FinalTestResult>();
    }
}
