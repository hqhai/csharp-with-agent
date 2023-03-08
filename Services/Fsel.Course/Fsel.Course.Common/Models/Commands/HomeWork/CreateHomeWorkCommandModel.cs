using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.HomeWork
{
    public class CreateHomeWorkCommandModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public string? MediaPost { get; set; }
        public bool IsActive { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}