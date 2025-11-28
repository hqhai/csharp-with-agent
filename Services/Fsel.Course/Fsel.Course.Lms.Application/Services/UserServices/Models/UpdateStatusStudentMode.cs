// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Shared.Enums;

    public class UpdateStatusStudentMode
    {
        public Guid StudentId { get; set; }
        public EnumStatusStudentCampus StatusStudentGoal { get; set; }
    }
}
