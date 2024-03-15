// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class TokenConfigModel
    {
        public Guid Id { get; set; }
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public object? Config { get; set; }
    }
}
