// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    public class CreateClassStudentModel
    {
        public string? Code { get; set; }
        public Guid? ClassId { get; set; }
        public Guid UserId { get; set; }
    }
}
