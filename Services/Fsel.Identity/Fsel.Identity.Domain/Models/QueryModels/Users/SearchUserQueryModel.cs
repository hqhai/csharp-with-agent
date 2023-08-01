// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.Users
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchUserQueryModel : BaseQueryModel
    {
        public EnumRoleRegisterWithAdmin Role { get; set; }
        public IList<EnumRoleTeacher>? RoleTeachers { get; set; }
    }
}
