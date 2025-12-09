// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Videos
{
    public class CreateVideoCommandModel
    {
        public string? Name { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public Guid TeacherId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? OriginalId { get; set; }
        public IList<VideoPercentConfig>? VideoPercentConfigs { get; set; }
        public IList<CreateVideoTimeCodeCommandModel>? VideoTimeCodes { get; set; }
    }
}
