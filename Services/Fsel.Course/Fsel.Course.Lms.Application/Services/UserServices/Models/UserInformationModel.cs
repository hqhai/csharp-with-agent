// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Shared.Enums;

    public class UserInformationModel
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public EnumUserStatus? Status { get; set; }
    }
}
