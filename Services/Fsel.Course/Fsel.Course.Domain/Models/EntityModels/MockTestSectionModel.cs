// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Entities;

    public class MockTestSectionModel : Entity
    {
        public MockTestModel? MockTest { get; set; }
        public SectionGroupModel? SectionGroup { get; set; }
        public Guid MockTestId { get; set; }

        public Guid SectionGroupId { get; set; }

        public IList<SectionGroupModel>? SectionGroups { get; set; }
    }
}
