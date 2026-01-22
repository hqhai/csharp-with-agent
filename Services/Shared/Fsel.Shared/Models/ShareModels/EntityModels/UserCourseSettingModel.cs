// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserCourseSettingModel : BaseModel
    {
        public EnumUserCourseType Type { get; set; }
        public int Value { get; set; }
        public Guid? LevelId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid UserId { get; set; }
    }
}
