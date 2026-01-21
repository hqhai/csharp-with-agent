// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SaveUserCourseSettingQueueModel
    {
        public EnumUserCourseType Type { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid? LevelId { get; set; }
        public bool IsDeduction { get; set; }
        public Guid UserId { get; set; }
    }
}
