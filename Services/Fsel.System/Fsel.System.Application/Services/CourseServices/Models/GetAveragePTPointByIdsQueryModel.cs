// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    public class GetAveragePTPointByIdsQueryModel
    {
        public IList<GetAveragePTPointByIdQueryModel> PointByIdQueryModels { get; set; } = new List<GetAveragePTPointByIdQueryModel>();
    }

    public class GetAveragePTPointByIdQueryModel
    {
        public Guid ClassId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}
