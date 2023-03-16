// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;

    public class CourseTeacherModel : BaseEntityModel
    {
        public Guid TeacherId { get; set; }
        public Guid CourseId { get; set; }
    }
}
