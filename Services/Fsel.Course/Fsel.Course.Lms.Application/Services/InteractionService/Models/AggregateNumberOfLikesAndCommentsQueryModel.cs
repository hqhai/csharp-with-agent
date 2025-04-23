// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using System;
    using System.Collections.Generic;

    public class AggregateNumberOfLikesAndCommentsQueryModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public IList<Guid>? ClassForumResultIds { get; set; }
    }
}
