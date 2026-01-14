// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class HomeWorkConfigModel : BaseModel
    {
        public string? HomeWorkName { get; set; }
        public int NumberRetry { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid HomeWorkId { get; set; }
        public Guid CurriculumId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public Guid? ProgramId { get; set; }
        public string? Program { get; set; }
        public Guid? LevelId { get; set; }
        public string? Level { get; set; }
        public Guid? SkillId { get; set; }
        public string? Skill { get; set; }
        public string? Subject { get; set; }

        public EnumCourseType CourseType
        {
            get { return EnumCourseLevelHelper.GetEnumCourseType(CourseLevel); }
        }
    }
}
