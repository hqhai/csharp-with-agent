using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class LessonModel : BaseEntityModel
    {
        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Tên bài test
        /// </summary>
        public string? Name { get; set; }

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
    }
}
