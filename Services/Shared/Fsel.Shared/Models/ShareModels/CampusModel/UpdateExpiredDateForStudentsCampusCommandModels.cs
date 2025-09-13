// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using System.Collections.Generic;

    public class UpdateExpiredDateForStudentsCampusCommandModels
    {
        public IList<UpdateExpiredDateForStudentsCampusCommandModel>? Students { get; set; }
    }

    public class UpdateExpiredDateForStudentsCampusCommandModel
    {
        public Guid StudentId { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
