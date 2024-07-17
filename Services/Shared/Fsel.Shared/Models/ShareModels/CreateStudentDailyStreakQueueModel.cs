// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using System.Text.Json.Serialization;

    public class CreateStudentDailyStreakQueueModel
    {
        [JsonIgnore]
        public Guid? StudentId { get; set; }

        [JsonIgnore]
        public bool IsUseShield { get; set; }

        [JsonIgnore]
        public DateTime? DailyDate { get; set; }

        public Guid UserId { get; set; }
    }
}

