using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.ClassForum
{
    public class UpdateClassForumCommandModel : BaseCommandModel
    {
        public string? Title { get; set; }

        public EnumGradingStyle GradingStyle { get; set; }

        public TimeSpan? TaggetTimeLimit { get; set; }
        public TimeSpan? TaggetWordLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
    }
}