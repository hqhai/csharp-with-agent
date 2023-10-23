// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Shared.Enums;

    public class PlacementTestResult : BaseResultScore
    {
        public EnumPlacementTestLevel Level { get; set; }
        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
    }
}