// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class UserSearchModel : BaseModel
    {
        public bool Status { get; set; }
        public HumanSearchModel? Human { get; set; }
    }
}
