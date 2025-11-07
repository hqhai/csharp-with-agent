// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.ClassForums.V1i1;
    using Fsel.Course.Domain.Models.CommandModels.Documents;
    using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;
    using Fsel.Shared.Enums;

    public class UpdateLessonCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }
        public Guid LevelId { get; set; }

        public Guid ProgramId { get; set; }

        public EnumStatus Status { get; set; }

        public IList<CreateLessonInstructionCommandModel>? LessonInstructions { get; set; }

        public IList<UpdateLessonModuleModel>? LessonModules { get; set; }

    }

    public class UpdateLessonModuleModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        public EnumLessonConfigType LessonConfigType { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid? OriginalId { get; set; }

        public CreateClassForumCommandModel? ClassForum { get; set; }

        public CreateDocumentCommandModel? Document { get; set; }
    }
}
