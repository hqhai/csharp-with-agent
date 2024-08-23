// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class AddExpiredDateForStudentQueueModel
    {
        public Guid StudentId { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
    }
}
