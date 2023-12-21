// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FinalTestAnswerModel : BaseAnswerModel
    {
        public Guid FinalTestResultId { get; set; }
        public Guid SectionQuestionId { get; set; }
    }
}
