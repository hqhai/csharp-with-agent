// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    public class GetBlindBoxesByUserIdsQueryModel
    {
        public IList<Guid>? UserIds { get; set; }
    }
}
