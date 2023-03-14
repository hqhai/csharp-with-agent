// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.ParentStudents;

    public class CreateParentCommandModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public Guid HumanId { get; set; }
        public CreateParentStudentCommandModel? Parention { get; set; }
    }
}
