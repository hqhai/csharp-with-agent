// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Core.Base.BaseModels;
    using Enums;

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

        public Guid LessonId { get; set; }

        public Guid OriginalId { get; set; }

        public bool IsLocked { get; set; }

        public bool IsDone { get; set; }
    }
}
