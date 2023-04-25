// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class HumanSearchModel : BaseModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public TeacherModel? Teacher { get; set; }
        public CSOModel? CSO { get; set; }
    }
}
