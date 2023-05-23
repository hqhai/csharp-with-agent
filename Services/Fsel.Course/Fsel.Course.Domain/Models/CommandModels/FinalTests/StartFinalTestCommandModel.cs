// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTests
{
    public class StartFinalTestCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid FinalTestId { get; set; }
    }
}
