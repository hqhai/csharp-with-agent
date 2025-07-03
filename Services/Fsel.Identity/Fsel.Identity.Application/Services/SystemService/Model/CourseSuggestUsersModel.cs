// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;

    public class CourseSuggestUsersModel
    {
        public Guid UserId { get; set; }

        public IList<CourseSuggestModel>? CourseSuggestConfigs { get; set; }
    }
}
