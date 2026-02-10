// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Models
{
    using System;

    public class ClassForumTranslationQueueModel
    {
        public Guid ClassForumDetailResultId { get; set; }
        public string? GradingAlFeedback { get; set; }
    }
}
