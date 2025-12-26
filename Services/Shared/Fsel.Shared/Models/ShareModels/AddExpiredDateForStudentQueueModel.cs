// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using Fsel.Shared.Enums;

    public class AddExpiredDateForStudentQueueModel
    {
        public EnumStudentEditHistoryType StudentEditHistoryType { get; set; }
        public Guid StudentId { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string? Description { get; set; }
    }
}
