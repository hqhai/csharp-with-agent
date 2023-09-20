// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumScores
{
    using System;
    using System.Collections.Generic;

    public class GradeClassForumCommandModel
    {
        public Guid ClassForumResultId { get; set; }
        public IList<CreateClassForumScoreCommandModel>? ClassForumScores { get; set; }
    }
}
