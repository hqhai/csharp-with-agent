// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SectionGroupModel : BaseModel
    {
        public double ExecutionTime { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        private string? _audioPath;

        public string? AudioPath
        {
            set { _audioPath = value; }
            get { return _audioPath.AddS3BaseUrl(); }
        }

        public string? SkillName { get; set; }
        public Guid? SkillId { get; set; }
        public long TotalQuestion { get; set; }
        public EnumResultStatus Status { get; set; }
        public IList<SectionModel>? Sections { get; set; }
        public IList<MockTestScoreModel>? MockTestScores { get; set; }
        public SectionGroupResultModel? SectionGroupResult { get; set; }
    }
}
