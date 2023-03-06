using Fsel.Common.ActionResults;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.PlacementTest
{
    public class CreatePlacementTestCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
