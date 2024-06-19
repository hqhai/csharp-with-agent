// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    public class GetListClassBySpecificStudentIdsQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }
    }
}
