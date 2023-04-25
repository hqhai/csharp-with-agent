// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.Users
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchUserQueryModel : BaseQueyModel
    {
        public EnumRoleRegisterWithAdmin Role { get; set; }
        public EnumTeacherRole? TeacherRole { get; set; }
        public EnumCSORole? CSORole { get; set; }
    }
}
