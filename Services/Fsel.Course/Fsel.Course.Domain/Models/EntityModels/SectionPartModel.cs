// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionPartModel
    {
        public Guid Id { get; set; }
        public string? PartName { get; set; }
        public Guid SectionId { get; set; }
        public IList<QuestionModel>? Questions { get; set; }
    }
}
