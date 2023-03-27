// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    public class UpdateStudentByClassCommandModel
    {
        public Guid ClassId { get; set; }
        public Guid UserId { get; set; }
    }
}
