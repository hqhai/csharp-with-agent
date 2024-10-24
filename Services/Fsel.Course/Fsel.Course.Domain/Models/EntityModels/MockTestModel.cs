// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class MockTestModel : BaseModel
    {
        public double Version { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public EnumMockTestType MockTestType { get; set; }
        public long TotalQuestion { get; set; }
        public double ExecutionTime { get; set; }
        public IList<SectionGroupModel>? SectionGroups { get; set; }
        public MockTestResultModel? MockTestResult { get; set; }
        public EnumCourseSkill? Skill { get; set; }

        public IList<EnumCourseSkill>? CourseSkills
        {
            get
            {
                return SectionGroups?.Select(x => x.CourseSkill).ToList();
            }
        }

        public string? PostArea { get; set; }
    }
}
