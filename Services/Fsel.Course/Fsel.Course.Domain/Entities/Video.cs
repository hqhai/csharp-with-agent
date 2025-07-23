// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Video : Entity
    {
        /// <summary>
        /// Tên Video
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumVideoErrorCode.InvalidKeywordCharacter))]
        public string? Name { get; set; }

        /// <summary>
        /// Link Video
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? VideoFilePath { get; set; }

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

        public EnumVersionStatus VersionStatus { get; set; }
        public int Version { get; set; }
        public Guid? OriginalId { get; set; }

        public Guid? LevelId { get; set; }
        public Level? Level { get; set; }

        public Guid? ProgramId { get; set; }
        public Category? Program { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public ICollection<LessonVideo> LessonVideos { get; set; } = new List<LessonVideo>();
        public ICollection<VideoTimeCode> VideoTimeCodes { get; set; } = new List<VideoTimeCode>();
        public ICollection<VideoResult> VideoResults { get; set; } = new List<VideoResult>();
        public ICollection<LessonModule> LessonModules { get; set; } = new List<LessonModule>();
    }
}