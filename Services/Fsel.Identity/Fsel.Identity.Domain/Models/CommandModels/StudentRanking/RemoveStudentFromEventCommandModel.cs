// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.StudentRanking
{
    public class RemoveStudentFromEventCommandModel
    {
        public string? EventCode { get; set; }
        public IList<string>? Emails { get; set; }
    }
}
