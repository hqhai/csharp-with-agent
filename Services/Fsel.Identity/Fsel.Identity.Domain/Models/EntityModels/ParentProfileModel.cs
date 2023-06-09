// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class ParentProfileModel
    {
        public Guid Id { get; set; }
        public string? Occupation { get; set; }
        public HumanProfileModel? Human { get; set; }
    }
}
