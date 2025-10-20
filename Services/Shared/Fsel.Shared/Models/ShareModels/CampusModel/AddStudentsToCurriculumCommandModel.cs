// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    public class AddStudentsToCurriculumCommandModel
    {
        public Guid CurriculumId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}
