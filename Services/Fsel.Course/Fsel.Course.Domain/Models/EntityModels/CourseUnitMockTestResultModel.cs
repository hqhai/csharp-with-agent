// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class CourseUnitMockTestResultModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? MockTestId { get; set; }

        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public double ProgressPercent { get; set; }
        public int DisplayOrder { get; set; }
    }
}
