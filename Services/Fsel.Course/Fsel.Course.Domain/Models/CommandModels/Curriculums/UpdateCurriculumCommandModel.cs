// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Curriculums
{
    using System;

    public class UpdateCurriculumCommandModel
    {
        public Guid Id { get; set; }
        public string? CurriculumName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
