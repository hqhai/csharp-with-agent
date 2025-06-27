// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using Fsel.Shared.Enums;

    public class AddExpiredDateForStudentCommandModel
    {
        public Guid StudentId { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string? Description { get; set; }
        public EnumStudentEditHistoryType StudentEditHistoryType { get; set; }
    }
}
