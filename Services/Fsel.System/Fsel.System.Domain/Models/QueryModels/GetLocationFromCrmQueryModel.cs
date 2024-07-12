// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.Entities;

    public class GetLocationFromCrmQueryModel : BaseQueryModel
    {
        public int Level { get; set; }
        public int? ParentID { get; set; }
        public EnumCrmLocationTypeLevel? TypeLevel { get; set; }
    }
}
