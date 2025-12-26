// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorks
{
    public class UpdateHomeWorkCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? MediaPost { get; set; }
        public string? MediaPostContentRuby { get; set; }
        public Guid? SkillId { get; set; }
        public IList<CreateQuestionCommandModel>? Questions { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? ProgramId { get; set; }
    }
}
