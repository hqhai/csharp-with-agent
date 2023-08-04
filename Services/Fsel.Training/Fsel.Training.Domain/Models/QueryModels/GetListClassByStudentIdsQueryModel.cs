// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    public class GetListClassByStudentIdsQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }
        public Guid? CourseId { get; set; }
    }
}
