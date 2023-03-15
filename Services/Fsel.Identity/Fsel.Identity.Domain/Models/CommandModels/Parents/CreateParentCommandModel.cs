// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System;
    using Fsel.Identity.Domain.Models.CommandModels.ParentStudents;

    public class CreateParentCommandModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public Guid HumanId { get; set; }
        public CreateParentStudentCommandModel? Parention { get; set; }
    }
}
