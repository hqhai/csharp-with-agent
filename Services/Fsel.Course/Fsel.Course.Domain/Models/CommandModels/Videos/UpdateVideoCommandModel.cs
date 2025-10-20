// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Videos
{
    public class UpdateVideoCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public EnumVideoType Type { get; set; }
        public Guid TeacherId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<CreateVideoTimeCodeCommandModel>? VideoTimeCodes { get; set; }
        public IList<CreateVideoSubFilePathCommandModel>? VideoSubFilePaths { get; set; }
    }
}
