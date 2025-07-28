// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.InterationActions
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
