// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class UserGroupMemberShipModel : BaseModel
    {
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
        public Guid GroupId { get; set; }
        public bool IsActive { get; set; }
    }
}
