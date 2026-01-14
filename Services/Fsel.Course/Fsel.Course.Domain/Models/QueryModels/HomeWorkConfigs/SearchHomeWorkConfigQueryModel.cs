// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.HomeWorkConfigs
{
    using Fsel.Core.Base.BaseModels;

    public class SearchHomeWorkConfigQueryModel : BaseQueryModel
    {
        public Guid CurriculumId { get; set; }
        public IList<Guid>? CreatedUserIds { get; set; }
        public IList<Guid>? ProgramIds { get; set; }
        public IList<Guid>? SkillIds { get; set; }
        public IList<Guid>? LevelIds { get; set; }
    }
}
