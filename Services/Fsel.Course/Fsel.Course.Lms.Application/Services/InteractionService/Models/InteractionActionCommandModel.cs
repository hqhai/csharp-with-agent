// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using System;
    using System.Collections.Generic;

    public class InteractionActionCommandModel
    {
        public IList<Guid>? ObjectIds { get; set; }

        public Guid? UserId { get; set; }
    }
}
