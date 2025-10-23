// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using System.Collections.Generic;

    public class GetStudentsLearningProgressQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }
    }
}
