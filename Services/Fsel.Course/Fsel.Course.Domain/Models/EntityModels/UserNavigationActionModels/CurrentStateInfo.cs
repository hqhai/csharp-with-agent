// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels
{
    public class CurrentStateInfo
    {
        public Guid? PtResultId { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? CourseResultId { get; set; }
    }
}
