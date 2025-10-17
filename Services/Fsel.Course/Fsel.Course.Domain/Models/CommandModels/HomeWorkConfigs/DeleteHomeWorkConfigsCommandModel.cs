// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorkConfigs
{
    using System;
    using System.Collections.Generic;

    public class DeleteHomeWorkConfigsCommandModel
    {
        public IList<Guid>? HomeWorkConfigIds { get; set; }
    }
}
