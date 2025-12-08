// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface ICourseItemInitializer
    {
        Task<VoidMethodResult> InitializeAsync(CourseModule courseModule, CourseResult courseResult, CancellationToken cancellationToken);
    }
}
