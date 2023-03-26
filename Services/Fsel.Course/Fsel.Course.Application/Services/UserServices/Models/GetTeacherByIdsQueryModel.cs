// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.UserServices.Models
{
    using System;
    using System.Collections.Generic;

    public class GetTeacherByIdsQueryModel
    {
        public IList<Guid>? Ids { get; set; }
    }
}