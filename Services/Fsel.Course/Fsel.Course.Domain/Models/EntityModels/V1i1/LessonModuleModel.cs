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

        public Guid? VideoId { get; set; }

        public VideoModel? Video { get; set; }

        public Guid? ClassForumId { get; set; }

        public ClassForumModel? ClassForum { get; set; }

        public Guid? HomeWorkId { get; set; }

        public HomeWorkModel? HomeWork { get; set; }

        public Guid? DocumentId { get; set; }

        public DocumentModel? Document { get; set; }
    }
}
