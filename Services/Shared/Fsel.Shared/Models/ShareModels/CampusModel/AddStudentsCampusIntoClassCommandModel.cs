// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using System.Collections.Generic;

    public class AddStudentsCampusIntoClassCommandModel
    {
        public IList<Guid>? StudentIds { get; set; }
        public Guid CourseId { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
