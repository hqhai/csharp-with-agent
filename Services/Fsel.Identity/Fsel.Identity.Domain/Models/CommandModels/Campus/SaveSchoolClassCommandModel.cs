// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Campus
{
    using System;

    public class SaveSchoolClassCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public Guid? TeacherId { get; set; }
    }
}
