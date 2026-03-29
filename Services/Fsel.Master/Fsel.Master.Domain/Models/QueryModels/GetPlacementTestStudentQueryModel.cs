// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class GetPlacementTestStudentQueryModel : BaseQueryModel
    {
        public IList<Guid>? ProvinceIds { get; set; }
        public IList<Guid>? DistrictIds { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
        public Guid SubjectId { get; set; }
        public IList<Guid>? LevelIds { get; set; }
        public IList<Guid>? ProgramIds { get; set; }
    }
}
