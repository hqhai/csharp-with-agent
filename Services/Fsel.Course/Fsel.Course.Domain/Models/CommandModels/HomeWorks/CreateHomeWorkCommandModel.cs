// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorks
{
    public class CreateHomeWorkCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? MediaPost { get; set; }
        public string? MediaPostContentRuby { get; set; }
        public IList<CreateQuestionCommandModel>? Questions { get; set; }
        public Guid? SkillId { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? ProgramId { get; set; }
        public bool IsActive { get; set; }
        public EnumHomeWorkType Type { get; set; }
    }
}
