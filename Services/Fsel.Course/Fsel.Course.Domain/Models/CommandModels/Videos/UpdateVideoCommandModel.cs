// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;

namespace Fsel.Course.Domain.Models.CommandModels.Videos
{
    public class UpdateVideoCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? VideoFilePath { get; set; }

        public Guid TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public IList<UpdateVideoTimeCodeCommandModel>? VideoTimeCodes { get; set; }
    }
}
