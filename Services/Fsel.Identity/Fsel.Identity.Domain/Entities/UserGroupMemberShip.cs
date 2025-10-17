// Copyright (c) Atlantic. All rights reserved.


using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class UserGroupMemberShip : Entity
    {
        // Thiết lập mối quan hệ với User
        public Guid UserId { get; set; }
        //public virtual User? User { get; set; }

        // Thiết lập mối quan hệ với UserGroup
        public Guid GroupId { get; set; }
        //public virtual UserGroup? Group { get; set; }

        // Thông tin về thành viên trong nhóm
        public bool IsActive { get; set; } = true;
    }
}
