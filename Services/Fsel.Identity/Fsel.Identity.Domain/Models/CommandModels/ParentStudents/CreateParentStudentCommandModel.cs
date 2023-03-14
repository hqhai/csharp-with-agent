// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.ParentStudents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateParentStudentCommandModel
    {
        public Guid ParentId { get; set; }
        public Guid StudentId { get; set; }
    }
}
