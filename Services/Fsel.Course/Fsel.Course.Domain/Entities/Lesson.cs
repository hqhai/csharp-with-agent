// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Lesson : Entity, IVersionEntity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        /// <summary>
        /// số video
        /// </summary>
        public int VideoCount { get; set; }

        /// <summary>
        /// số bài classforum
        /// </summary>
        public int ClassForumCount { get; set; }

        /// <summary>
        /// số bài homewwork
        /// </summary>
        public int HomeWorkCount { get; set; }

        /// <summary>
        /// số bài doc
        /// </summary>
        public int DocumentCount { get; set; }

        public EnumStatus Status { get; set; }

        public Guid? LevelId { get; set; }

        public Level? Level { get; set; }

        public Guid? ProgramId { get; set; }

        public Category? Category { get; set; }

        public Guid OriginalId { get; set; }

        public int Version { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }

        public ICollection<LessonVideo> LessonVideos { get; set; } = new List<LessonVideo>();
        public ClassForum? ClassForum { get; set; }
        public ICollection<LessonHomeWork> LessonHomeWorks { get; set; } = new List<LessonHomeWork>();
        public ICollection<LessonExtraPractice> LessonExtraPractices { get; set; } = new List<LessonExtraPractice>();
        public ICollection<UnitLesson> UnitLessons { get; set; } = new List<UnitLesson>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
        public ICollection<LessonInstruction> LessonInstructions { get; set; } = new List<LessonInstruction>();
        public ICollection<LessonModule> LessonModules { get; set; } = new List<LessonModule>();
    }
}
