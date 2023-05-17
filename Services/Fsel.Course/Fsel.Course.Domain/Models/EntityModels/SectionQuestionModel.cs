// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class SectionQuestionModel : BaseModel
    {
        public Guid QuestionId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? SecpartId { get; set; }
    }
}
