// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class HomeWork : Entity, IVersionEntity
    {
        /// <summary>
        /// Tên bài tập
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegularExpression(@"^[^<>,]{1,200}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Name { get; set; }

        /// <summary>
        ///  Code
        /// </summary>
        ///
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegularExpression(@"^[^<>,]{1,200}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Code { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        public string? MediaPost { get; set; }

        /// <summary>
        /// Media Post Ruby
        /// </summary>

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? MediaPostContentRuby { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }
        public int Version { get; set; }

        public Guid OriginalId { get; set; }

        public Guid? LevelId { get; set; }
        public Level? Level { get; set; }

        public Guid? ProgramId { get; set; }
        public Category? Program { get; set; }

        public ICollection<HomeWorkResult> HomeWorkResults { get; set; } = new List<HomeWorkResult>();
        public ICollection<LessonHomeWork> LessonHomeWorks { get; set; } = new List<LessonHomeWork>();

        public ICollection<HomeWorkQuestion> HomeWorkQuestions { get; set; } = new List<HomeWorkQuestion>();
    }
}
