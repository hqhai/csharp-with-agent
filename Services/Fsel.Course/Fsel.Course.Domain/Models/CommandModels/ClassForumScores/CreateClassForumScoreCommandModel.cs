// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumScores
{
    using Fsel.Course.Domain.Enums;

    public class CreateClassForumScoreCommandModel
    {
        public string? Feedback { get; set; }

        public long? Score { get; set; }

        public Guid ClassForumResultId { get; set; }

        public EnumClassForumScoreCriteria Criteria { get; set; }
    }
}
