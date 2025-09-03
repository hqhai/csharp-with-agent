// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels.IntegrationModels
{
    public class CourseSuggestUsersModel
    {
        public Guid UserId { get; set; }

        public IList<CourseSuggestConfigStudentModel> CourseSuggestConfigs { get; set; } = new List<CourseSuggestConfigStudentModel>();
    }
}
