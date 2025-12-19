// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public class CachedSectionTreeModel
    {
        public IList<TestSection> Sections { get; init; } = new List<TestSection>();
        public Dictionary<Guid, List<Guid>> QuestionIdsBySectionId { get; init; } = new();
    }
}
