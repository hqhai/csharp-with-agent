// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class DeleteGuestStudentQueueModel : BaseModel
    {
        public string? Membership { get; set; }

        public string? Occupation { get; set; }

        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? ClassId { get; set; }
    }
}
