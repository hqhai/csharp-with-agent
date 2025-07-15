// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class ClassForum : Entity, IVersionEntity
    {
        /// <summary>
        /// Cách chấm điểm
        /// </summary>
        public EnumGradingStyle GradingStyle { get; set; }

        [MaxLength(300, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PromptName { get; set; }

        /// <summary>
        /// Số từ giới hạn
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public long TaggetWordLimit { get; set; }

        /// <summary>
        /// Thời gian giới hạn
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
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

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public Guid? LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public bool IsAlFeedBack { get; set; }

        //[RequiredIf(nameof(IsAlFeedBack), true, ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(5000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SystemRoleAlConfig { get; set; }

        //[RequiredIf(nameof(IsAlFeedBack), true, ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(5000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UserAlConfig { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingModel { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTemperature { get; set; }

        [Range(0, 4095, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingWordMaxLength { get; set; }

        [Range(0, 1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTopP { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingFrequecy { get; set; }

        [Range(0, 2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingPresence { get; set; }

        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }

        public Guid? ProgramId { get; set; }
        public Category? Category { get; set; }

        public Guid OriginalId { get; set; }

        public int Version { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }

        public EnumClassForumLayout Layout { get; set; }

        public ICollection<ClassForumResult> ClassForumResults { get; set; } = new List<ClassForumResult>();

        public ICollection<ClassForumResultRandom> ClassForumResultRandoms { get; set; } = new List<ClassForumResultRandom>();

        public ICollection<ClassForumFile> ClassForumFiles { get; set; } = new List<ClassForumFile>();
    }
}
