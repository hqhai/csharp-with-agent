// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class HumanModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Code { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public EnumGender? Gender { get; set; }
        public string? Email { get; set; }
        public Guid? ManageUserId { get; set; }
        public string? ManageUserName { get; set; }

        private string? _avatarPath;

        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public Guid? UserId { get; set; }
        public Guid? CourseId { get; set; }
        public TeacherModel? Teacher { get; set; }
        public CSOModel? CSO { get; set; }
        public StudentModel? Student { get; set; }
        public ParentProfileModel? Parent { get; set; }
    }
}
