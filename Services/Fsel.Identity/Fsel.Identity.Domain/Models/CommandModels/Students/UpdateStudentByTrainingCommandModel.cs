// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    public class UpdateStudentByTrainingCommandModel
    {
        public Guid ClassId { get; set; }
        public Guid UserId { get; set; }
    }
}
