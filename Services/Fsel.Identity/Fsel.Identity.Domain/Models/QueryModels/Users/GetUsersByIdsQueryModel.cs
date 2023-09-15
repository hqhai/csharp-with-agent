// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.Users
{
    public class GetUsersByIdsQueryModel
    {
        public IList<string>? UserIds { get; set; }
    }
}
