// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.UserGroup
{
    using Fsel.Core.Base.BaseModels;

    public class GetUserGroupQueryModel : BaseQueryModel
    {
        public bool? IsActive { get; set; }
    }
} 
