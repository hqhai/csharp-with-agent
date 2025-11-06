// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.CommandModels
{
    using System;
    using System.Collections.Generic;

    public class UpdateExpiredDateForStudentsEventCommandModel
    {
        public IList<Guid>? StudentIds { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
