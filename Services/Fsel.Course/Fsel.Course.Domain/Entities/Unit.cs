using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
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
       

        [Required]
        [MaxLength(250)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(250)]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
