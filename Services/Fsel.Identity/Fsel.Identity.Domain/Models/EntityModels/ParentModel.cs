// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ParentModel : BaseModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public Guid HumanId { get; set; }
        public IList<ParentStudentModel>? ParentStudents { get; set; }
    }
}
