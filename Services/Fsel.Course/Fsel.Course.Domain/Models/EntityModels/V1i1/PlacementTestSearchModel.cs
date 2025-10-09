// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    public class PlacementTestSearchModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Name { get; set; }
        public Guid? LevelId { get; set; }
        public Guid OriginalId { get; set; }
        public string? LevelName { get; set; }
        public string? LevelCode { get; set; }
    }
}
