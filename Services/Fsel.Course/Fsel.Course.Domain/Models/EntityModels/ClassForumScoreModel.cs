// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.Enums;

    public class ClassForumScoreModel
    {
        public Guid Id { get; set; }
        public string? Feedback { get; set; }
        public Guid ClassForumResultId { get; set; }
        public long? Score { get; set; }
        public EnumClassForumScoreCriteria Criteria { get; set; }
    }
}
