// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using System;

    public class StudentConsecutiveDayModel
    {
        public Guid Id { get; set; }
        public DateTime DailyDate { get; set; }
        public Guid StudentId { get; set; }
    }
}
