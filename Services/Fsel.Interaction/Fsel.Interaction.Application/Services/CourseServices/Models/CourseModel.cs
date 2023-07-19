// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? InstructionContent { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseType CourseType { get; set; }
    }
}
