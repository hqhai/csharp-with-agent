using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.Lesson
{
    public class UpdateLessonCommandModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? InstructionContent { get; set; }
        public bool IsActive { get; set; }
        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
