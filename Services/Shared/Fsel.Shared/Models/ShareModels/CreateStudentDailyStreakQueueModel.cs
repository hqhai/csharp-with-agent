// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class CreateStudentDailyStreakQueueModel
    {
        public Guid StudentId { get; set; }
        public bool IsUseShield { get; set; }
    }
}
