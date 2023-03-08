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
    public class HomeWork : Entity
    {
        [Required(ErrorMessage = nameof(EnumHomeWorkErrorCode.HW01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumHomeWorkErrorCode.HW02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumHomeWorkErrorCode.HW03C))]
        public string? InstructionContent { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumHomeWorkErrorCode.HW03C))]
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public List<LessonHomeWork> LessonHomeWorks { get; set; } = new List<LessonHomeWork>();
    }
}