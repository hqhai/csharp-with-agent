// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class PlacementTestGroupStudentResultModel
    {
        public Guid StudentId { get; set; }
        public PlacementTestResultModel? PlacementTestStart { get; set; }
        public PlacementTestResultModel? PlacementTestEnd { get; set; }
    }
}
