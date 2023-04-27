// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class UserModel : BaseModel
    {
        public string? FullName { get; set; }
        public Guid StudentId { get; set; }
        public Guid HumanId { get; set; }
        public HumanModel? Human { get; set; }
    }
}
