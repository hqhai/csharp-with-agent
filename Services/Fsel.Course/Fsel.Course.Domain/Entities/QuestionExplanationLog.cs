// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class QuestionExplanationLog : Entity
    {
        public string? PromptRequest { get; set; }
        public string? PromptResponse { get; set; }
        public Question? Question { get; set; }
        public Guid QuestionId { get; set; }
    }
}
