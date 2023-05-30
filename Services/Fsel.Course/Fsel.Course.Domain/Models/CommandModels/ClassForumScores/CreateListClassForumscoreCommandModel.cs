// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumScores
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.EntityModels;

    public class CreateListClassForumscoreCommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public IList<CreateClassForumScoreCommandModel>? ClassForumScores { get; set; }

        public bool IsSubmit { get; set; }
    }
}
