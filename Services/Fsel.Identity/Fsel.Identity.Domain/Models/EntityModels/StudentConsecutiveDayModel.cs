// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;

    public class StudentConsecutiveDayModel
    {
        public Guid Id { get; set; }
        public DateTime DailyDate { get; set; }
        public bool IsReceiveGift { get; set; }
        public int LevelOfGift { get; set; }
    }
}
