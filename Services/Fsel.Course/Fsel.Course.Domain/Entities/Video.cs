// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Domain.Entities
{
    public class Video : Entity
    {
        /// <summary>
        /// Tên Video
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        private string? _videoFilePath;

        /// <summary>
        /// Link Video
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? VideoFilePath
        {
            get { return _videoFilePath; }
            set { _videoFilePath = value; TimeCount = MediaHelper.GetMediaDurationAsync(value); }
        }

        private int? _timeCount;

        public int? TimeCount
        {
            get { return _timeCount == null ? MediaHelper.GetMediaDurationAsync(VideoFilePath) : _timeCount; }
            set { _timeCount = value; }
        }

        /// <summary>
        /// Sub File Path
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SubFilePath { get; set; }

        /// <summary>
        /// Loại Video
        /// </summary>
        public EnumVideoType Type { get; set; }

        /// <summary>
        /// Giáo Viên ID
        /// </summary>
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public ICollection<LessonVideo> LessonVideos { get; set; } = new List<LessonVideo>();
        public ICollection<VideoTimeCode> VideoTimeCodes { get; set; } = new List<VideoTimeCode>();
        public ICollection<VideoResult> VideoResults { get; set; } = new List<VideoResult>();
    }
}
