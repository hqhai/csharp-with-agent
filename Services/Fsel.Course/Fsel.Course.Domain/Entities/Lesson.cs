using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class Lesson : Entity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumLessonErrorCode.LS02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumLessonErrorCode.LS03C))]
        public string? InstructionContent { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumLessonErrorCode.LS02C))]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Giáo Viên ID
        /// </summary>
        public Guid? TeacherId { get; set; }
        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }
        public List<LessonVideo> lessonVideos { get; set; } = new List<LessonVideo>();

        public List<LessonUnit> lessonUnits { get; set; } = new List<LessonUnit>();
    }
}
