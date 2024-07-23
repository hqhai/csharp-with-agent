// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    public class StudentConsecutiveDayModel
    {
        public Guid Id { get; set; }
        public DateTime DailyDate { get; set; }
        public bool IsGiftReceive { get; set; }
        public int LevelOfGift { get; set; }
        public Guid StudentId { get; set; }
    }
}
