// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class SectionPartQuestionModel : BaseModel
    {
        public string? PartName { get; set; }
        public Guid SectionId { get; set; }
        public Guid QuestionId { get; set; }
    }
}
