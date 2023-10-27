// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.ParentStudents
{
    using System;

    public class CreateParentStudentCommandModel
    {
        public Guid ParentId { get; set; }
        public Guid StudentId { get; set; }
    }
}
