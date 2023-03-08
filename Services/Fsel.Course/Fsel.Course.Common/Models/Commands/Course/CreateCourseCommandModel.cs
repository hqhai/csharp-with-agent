using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Common.Models.Commands.Course
{
    public class CreateCourseCommandModel
    {
        public string? Name { get; set; }

        public int NumberOfUnits { get; set; }

        public int NumberOfLessons { get; set; }

        public bool IsPublish { get; set; }

        public List<Guid>? UnitIds { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}