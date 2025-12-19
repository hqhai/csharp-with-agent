// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class HomeWorkSearchModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? MediaPost { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public EnumVersionStatus VersionStatus { get; set; }
        public EnumHomeWorkType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public EnumCourseType CourseType
        {
            get { return EnumCourseLevelHelper.GetEnumCourseType(CourseLevel); }
        }

        public Guid? SkillId { get; set; }
        public Guid? ProgramId { get; set; }
        public string? Program { get; set; }
        public Guid? LevelId { get; set; }
        public Guid OriginalId { get; set; }
        public string? Level { get; set; }
        public string? SkillName { get; set; }
    }
}
