// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class LessonModel : BaseModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public bool IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
