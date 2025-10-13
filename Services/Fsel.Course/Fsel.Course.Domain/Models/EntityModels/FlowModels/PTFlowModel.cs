// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.FlowModels
{
    using System;

    public class PTFlowModel
    {
        public string Name { get; set; }

        public Guid FlowId { get; set; }

        public Guid? TestGroupResultId { get; set; }

        public int? MaxNumberOfModules { get; set; }
    }
}
