// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos
{
    public class GetLevelOfStudentsByStudentIdsQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }
    }
}
