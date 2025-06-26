// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoModel : BaseModel
    {
        public string? Name { get; set; }

        private string? _videoFilePath;

        public string? VideoFilePath
        {
            set { _videoFilePath = value; }
            get { return _videoFilePath.AddS3BaseUrl(); }
        }

        private string? _subFilePath;

        public string? SubFilePath
        {
            set { _subFilePath = value; }
            get { return _subFilePath.AddS3BaseUrl(); }
        }

        public bool IsActive { get; set; }
        public int TotalQuestion { get; set; }
        public EnumVideoType Type { get; set; }
        public Guid? TeacherId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<VideoTimeCodeModel>? VideoTimeCodes { get; set; }
        public IList<VideoSubFilePathModel>? VideoSubFilePaths { get; set; }
        public VideoResultModel? VideoResult { get; set; }
    }
}
