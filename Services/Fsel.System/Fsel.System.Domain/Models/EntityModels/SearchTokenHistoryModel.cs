// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchTokenHistoryModel : BaseModel
    {
        public double? TotalToken { get; set; }
        public double? ReciveToken { get; set; }
        public double? UsedToken { get; set; }
    }
}
