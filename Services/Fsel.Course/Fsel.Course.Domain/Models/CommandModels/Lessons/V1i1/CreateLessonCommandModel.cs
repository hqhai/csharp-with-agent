// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1
{
    using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;
    using Fsel.Course.Domain.Models.CommandModels.LessonModules;

    public class CreateLessonCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public Guid LevelId { get; set; }

        public Guid ProgramId { get; set; }

        public IList<CreateLessonInstructionCommandModel>? LessonInstructions { get; set; }

        public IList<CreateLessonModuleCommandModel>? LessonModules { get; set; }
    }
}
