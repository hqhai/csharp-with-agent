// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CompetitionClassStudentModel
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }
}
