// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface IUnitItemInitializer
    {
        Task<VoidMethodResult> InitializeAsync(UnitModule unitModule, UnitResult unitResult, CancellationToken cancellationToken);
    }
}
