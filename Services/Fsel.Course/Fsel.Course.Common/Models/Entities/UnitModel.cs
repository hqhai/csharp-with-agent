using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Entities
{
    public class UnitModel : BaseEntityModel
    {

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
