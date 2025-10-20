// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    public class HomeWorkExtraPracticeAnswer : BaseAnswer
    {
        public Question? Question { get; set; }
        public HomeWorkExtraPracticeResult? HomeWorkExtraPracticeResult { get; set; }
        public Guid QuestionId { get; set; }
        public Guid HomeWorkExtraPracticeResultId { get; set; }
    }
}
