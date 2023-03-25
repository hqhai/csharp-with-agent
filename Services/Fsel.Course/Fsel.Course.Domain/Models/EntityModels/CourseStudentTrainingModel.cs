// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class CourseStudentTrainingModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }
}
