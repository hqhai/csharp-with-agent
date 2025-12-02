// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class LessonModuleModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        public EnumLessonConfigType LessonConfigType { get; set; }

        public int DisplayOrder { get; set; }

        public int DisplayNumber { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid OriginalId { get; set; }

        public VideoModel? Video { get; set; }

        public ClassForumModel? ClassForum { get; set; }

        public HomeWorkModel? HomeWork { get; set; }

        public DocumentModel? Document { get; set; }

        public bool IsClassForumLock { get; set; } = true;

        public bool IsHomeWorkLock { get; set; } = true;

        public bool IsDocumentLock { get; set; } = true;
    }
}
