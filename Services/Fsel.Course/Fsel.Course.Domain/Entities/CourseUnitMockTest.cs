// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class CourseUnitMockTest : Entity
    {
        /// <summary>
        /// Số thứ tự
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DisplayOrder { get; set; }

        public Unit? Unit { get; set; }
        public Course? Course { get; set; }
        public MockTest? MockTest { get; set; }
        public FinalTest? FinalTest { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? FinalTestId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? MockTestId { get; set; }

        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
    }
}
