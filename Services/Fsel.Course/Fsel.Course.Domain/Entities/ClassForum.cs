// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class ClassForum : Entity
    {
        /// <summary>
        /// Cách chấm điểm
        /// </summary>
        public EnumGradingStyle GradingStyle { get; set; }

        /// <summary>
        /// Số từ giới hạn
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public long TaggetWordLimit { get; set; }

        /// <summary>
        /// Thời gian giới hạn
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double TaggetTimeLimit { get; set; }

        [NotMapped]
        public TimeSpan TaggetTimeSpanLimit
        {
            get { return TimeSpan.FromSeconds(TaggetTimeLimit); }
        }

        /// <summary>
        /// Media Post
        /// </summary>
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public Guid LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public bool IsAlFeedBack { get; set; }

        [RequiredIf(nameof(IsAlFeedBack), true, ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? SystemRoleAlConfig { get; set; }

        [RequiredIf(nameof(IsAlFeedBack), true, ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? UserAlConfig { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingModel { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTemperature { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingWordMaxLength { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTopP { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingFrequecy { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingPresence { get; set; }

        public ICollection<ClassForumResult> ClassForumResults { get; set; } = new List<ClassForumResult>();

        public ICollection<ClassForumFile> ClassForumFiles { get; set; } = new List<ClassForumFile>();
    }
}
