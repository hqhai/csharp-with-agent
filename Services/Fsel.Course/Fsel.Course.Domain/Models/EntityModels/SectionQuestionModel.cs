// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionQuestionModel
    {
        public Guid QuestionId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? SecpartId { get; set; }
        public QuestionModel? Question { get; set; }
    }
}
