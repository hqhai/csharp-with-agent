using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fsel.Course.Domain.Entities
{
    public class PlacementTest : Entity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required]
        [MaxLength(250)]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(1000)]
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }
    }
}