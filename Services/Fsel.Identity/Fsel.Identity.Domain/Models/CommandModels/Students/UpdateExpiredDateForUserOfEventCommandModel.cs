// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System;

    public class UpdateExpiredDateForUserOfEventCommandModel
    {
        public Guid StudentId { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
