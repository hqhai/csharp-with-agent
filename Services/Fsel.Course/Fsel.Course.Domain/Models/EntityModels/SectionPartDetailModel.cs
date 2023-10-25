// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionPartDetailModel
    {
        public Guid Id { get; set; }
        public string? PartName { get; set; }
        public IList<Guid>? QuestionIds { get; set; }
    }
}
