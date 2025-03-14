// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.SystemService.Models
{
    public class GetLocationsByIdsQueryModel
    {
        public IList<Guid>? Ids { get; set; }
        public string? IdsStr { get; set; }
    }
}
