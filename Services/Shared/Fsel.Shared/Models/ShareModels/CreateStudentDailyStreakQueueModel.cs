// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using System.Text.Json.Serialization;

    public class CreateStudentDailyStreakQueueModel
    {
        public Guid StudentId { get; set; }
        public bool IsUseShield { get; set; }

        [JsonIgnore]
        public DateTime? DailyDate { get; set; }

        [JsonIgnore]
        public int? NumberOfShield { get; set; }
    }
}
