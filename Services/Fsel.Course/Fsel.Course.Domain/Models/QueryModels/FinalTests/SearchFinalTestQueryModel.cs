// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.FinalTests
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchFinalTestQueryModel : BaseQueryModel
    {
        public EnumFinalTestLevel? FinalTestLevel { get; set; }
    }
}
