// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentCourseModel : BaseModel
    {
        public string? ClassName { get; set; }
        public string? CourseName { get; set; }
        public string? Membership { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EnumClassStatus Status { get; set; }
    }
}
