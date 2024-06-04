// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SaveUserCourseSettingQueueModel
    {
        public EnumUserCourseType Type { get; set; }
        public bool IsDeduction { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public bool IsCreate { get; set; } = true;
        public Guid UserId { get; set; }
    }
}
