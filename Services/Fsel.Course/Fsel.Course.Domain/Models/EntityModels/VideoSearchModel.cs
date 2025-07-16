// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class VideoSearchModel : BaseModel
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

        public Guid? OriginalId { get; set; }
        public bool IsActive { get; set; }
        public string? LevelName { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<string>? Skills { get; set; }
        public IList<VideoExerciseSearchModel>? Exercises { get; set; }
    }
}
