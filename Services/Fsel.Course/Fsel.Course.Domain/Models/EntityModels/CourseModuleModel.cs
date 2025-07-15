// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;

    public class CourseModuleModel : BaseModel, IDisplayInfo
    {
        public EnumCourseConfigType CourseConfigType { get; set; }

        public int DisplayOrder { get; set; }

        public int DisplayNumber { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid OriginalId { get; set; }

        public string? UnitName { get; set; }

        public string? TestName { get; set; }
    }
}
