// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System;

    public class UpdateParentCommandModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public Guid HumanId { get; set; }
    }
}
