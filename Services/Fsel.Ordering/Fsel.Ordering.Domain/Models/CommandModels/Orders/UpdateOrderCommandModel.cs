// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using Fsel.Shared.Enums;

    public class UpdateOrderCommandModel
    {
        public Guid? UserId { get; set; }
        public DateTime? ExpireDate { get; set; }

        public Guid CourseId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public string? CodeCourse { get; set; }

    }
}
