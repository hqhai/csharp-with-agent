// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Curriculums
{
    public class CreateCurriculumCommandModel
    {
        public string? CurriculumName { get; set; }
        public Guid? CourseId { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
    }
}
