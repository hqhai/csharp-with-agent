// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;

    public class InfoCourseIntegrationModel
    {
        public Guid UserId { get; set; }

        public IList<CourseIntegrationModel>? InfoCourseIntegrationDetails { get; set; }
    }
}
