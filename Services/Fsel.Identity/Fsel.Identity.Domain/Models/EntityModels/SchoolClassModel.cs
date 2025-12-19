// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class SchoolClassModel : BaseModel
    {
        public string? Name { get; set; }
        public string? TeacherName { get; set; }
        public Guid? TeacherId { get; set; }
        public int NumberOfStudent { get; set; }
    }
}
