using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class PlacementTestModel : BaseEntityModel
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
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
