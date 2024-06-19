// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class MockTestSectionModel
    {
        public SectionGroupModel? SectionGroup { get; set; }
        public Guid MockTestId { get; set; }
        public Guid SectionGroupId { get; set; }
    }
}
