// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Core.Base.BaseModels;
    using Enums;

    public class LessonModel : BaseModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public Guid? LevelId { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        public string? CourseLevel { get; set; }

        public Guid UnitId { get; set; }

        public Guid ObjectId { get; set; }

        public Guid CourseId { get; set; }

        public Guid LessonResult { get; set; }

        public EnumResultStatus Status { get; set; }

        public int? DisplayOrder { get; set; }

        public int? Number => DisplayOrder;

        public bool IsLocked { get; set; }

        public List<LessonModuleModel> LessonModules { get; set; }
    }
}
