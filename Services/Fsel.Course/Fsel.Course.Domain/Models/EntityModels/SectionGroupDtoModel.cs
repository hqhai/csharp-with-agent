// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SectionGroupDtoModel : BaseModel
    {
        public double ExecutionTime { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        private string? _audioPath;
        public double Version { get; set; }

        public string? AudioPath
        {
            set { _audioPath = value; }
            get { return _audioPath.AddS3BaseUrl(); }
        }

        public string? SkillName { get; set; }
        public Guid? SkillId { get; set; }
        public long TotalQuestion { get; set; }
        public IList<SectionDtoModel>? Sections { get; set; }
        public SectionGroupResultModel? SectionGroupResult { get; set; }
    }
}
