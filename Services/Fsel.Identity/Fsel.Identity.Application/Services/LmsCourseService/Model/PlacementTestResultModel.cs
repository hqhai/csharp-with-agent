// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;
    using Fsel.Shared.Enums;

    public class PlacementTestResultModel
    {
        public Guid UserId { get; set; }

        public string? Status { get; set; }

        public EnumCourseLevel? Level { get; set; }

        public DateTime? DateEdit { get; set; }

        public IList<IntegrationPlacementTestResultModels>? PlacementTestResults { get; set; }
    }
}
