// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonModules
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.ClassForums.V1i1;
    using Fsel.Course.Domain.Models.CommandModels.Documents;

    public class CreateLessonModuleCommandModel
    {
        public Guid? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        public EnumLessonConfigType LessonConfigType { get; set; }

        public double Percent { get; set; }

        public int DisplayOrder { get; set; }

        public int OpenOrder { get; set; }

        public Guid? VideoId { get; set; }

        public Guid? HomeWorkId { get; set; }

        public CreateClassForumCommandModel? ClassForum { get; set; }

        public CreateDocumentCommandModel? Document { get; set; }
    }
}
