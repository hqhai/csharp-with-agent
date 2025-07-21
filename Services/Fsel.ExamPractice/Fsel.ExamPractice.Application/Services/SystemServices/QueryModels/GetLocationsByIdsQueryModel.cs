// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Services.SystemServices.QueryModels
{
    public class GetLocationsByIdsQueryModel
    {
        public IList<Guid>? Ids { get; set; }
        public string? IdsStr { get; set; }
    }
}
