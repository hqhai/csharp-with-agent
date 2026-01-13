// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.QueryModels
{
    public class SearchStudentsByUserIdsQueryModel
    {
        public IList<Guid>? UserIds { get; set; }
        public string? Keyword { get; set; }
    }
}
