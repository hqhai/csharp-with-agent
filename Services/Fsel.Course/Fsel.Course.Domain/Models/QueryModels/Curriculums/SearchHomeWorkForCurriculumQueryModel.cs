// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Curriculums
{
    using Fsel.Core.Base.BaseModels;

    public class SearchHomeWorkForCurriculumQueryModel : BaseQueryModel
    {
        public Guid CurriculumId { get; set; }
        public IList<Guid>? SubjectIds { get; set; }
        public IList<Guid>? ProgramIds { get; set; }
        public IList<Guid>? LevelIds { get; set; }
        public IList<Guid>? SkillIds { get; set; }
        public IList<Guid>? CourseIds { get; set; }
        public IList<Guid>? CreatedUserIds { get; set; }
    }
}
