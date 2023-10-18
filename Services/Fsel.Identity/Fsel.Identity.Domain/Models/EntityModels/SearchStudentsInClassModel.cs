// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class SearchStudentsInClassModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public string? Email { get; set; }
        public DateTime? BirthDay { get; set; }
        public double PTPoint { get; set; }
        public Guid? ClassId { get; set; }
    }
}
