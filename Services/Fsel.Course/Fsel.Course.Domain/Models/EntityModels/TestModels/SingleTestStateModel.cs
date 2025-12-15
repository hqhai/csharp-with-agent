// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Enums;
    using PlacementTestModels;

    public class SingleTestStateModel
    {
        public Guid? StudentId { get; set; }

        public Guid? TestGroupResultId { get; set; }

        public string? Level { get; set; }

        public EnumResultStatus? Status { get; set; }

        public List<BaseTestStateModel> TestStates { get; set; } = new List<BaseTestStateModel>();
    }
}
