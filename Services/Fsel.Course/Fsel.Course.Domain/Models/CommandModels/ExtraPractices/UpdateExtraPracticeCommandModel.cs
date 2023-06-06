// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPractices
{
    public class UpdateExtraPracticeCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? InstructionContent { get; set; }
        public IList<string>? FilePaths { get; set; }
        public string? VideoPath { get; set; }
        public string? Author { get; set; }
        public string? Abstract { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<UpdateExtraPracticeCommandModel>? ExtraPracticeChapters { get; set; }
    }
}
