// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class LessonModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? InstructionContent { get; set; }

        public int VideoCount { get; set; }

        public int ClassForumCount { get; set; }

        public int HomeWorkCount { get; set; }

        public int DocumentCount { get; set; }

        public EnumStatus Status { get; set; }

        public Guid? LevelId { get; set; }

        public string? NameLevel { get; set; }

        public Guid? ProgramId { get; set; }
        public Guid? ProjectId { get; set; }

        public string? NameProgram { get; set; }

        public Guid OriginalId { get; set; }

        public IList<LessonInstructionModel>? LessonInstructions { get; set; }

        public IList<LessonModuleModel>? LessonModules { get; set; }
    }
}
