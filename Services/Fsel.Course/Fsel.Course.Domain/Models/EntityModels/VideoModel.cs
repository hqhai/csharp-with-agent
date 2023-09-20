// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoModel : BaseModel
    {
        public string? Name { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public bool IsActive { get; set; }
        public int TotalQuestion { get; set; }
        public EnumVideoType Type { get; set; }
        public Guid? TeacherId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<VideoTimeCodeModel>? VideoTimeCodes { get; set; }
        public VideoResultModel? VideoResult { get; set; }
    }
}
