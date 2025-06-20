// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.V1i1
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;

    public class LessonModule : Entity, IDisplayInfo
    {
        public EnumLessonConfigType LessonConfigType { get; set; }
        public int DisplayOrder { get; set; }
        public int DisplayNumber { get; set; }
        public double Percent { get; set; }
        public int OpenOrder { get; set; }
        public Guid? LessonId { get; set; }
        public Guid? ObjectId { get; set; }
    }
}
