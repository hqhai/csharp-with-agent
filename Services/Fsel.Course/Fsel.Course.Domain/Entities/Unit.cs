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
    public class Unit : Entity
    {


        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumUnitErrorCode.U02C))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumUnitErrorCode.U02C))]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
