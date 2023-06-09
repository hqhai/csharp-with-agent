// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.PlacementTests
{
    public class SearchPlacementTestQueryModel : BaseQueryModel
    {
        public EnumPlacementTestLevel? Level { get; set; }
    }
}
