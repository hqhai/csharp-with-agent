// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels.IntegrationModels
{
    using Fsel.Shared.Enums;

    public class GetCourseSuggestByUserIdsQueryModel
    {
        public IList<GetCourseSuggestByUserIdsDetailModel>? GetCourseSuggestByUserIds { get; set; }
    }

    public class GetCourseSuggestByUserIdsDetailModel
    {
        public Guid UserId { get; set; }

        public int Age { get; set; }

        public EnumCourseLevel? BaseCourseLevel { get; set; }
    }
}
