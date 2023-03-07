using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class Video : Entity
    {
        /// <summary>
        /// Tên Video
        /// </summary>
        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumLessonErrorCode.LS02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumLessonErrorCode.LS03C))]
        public string? VideoFilePath { get; set; }

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
    }
}
