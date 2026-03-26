// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.QueryModels.StudentDashboard
{
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using MediatR;

    public class GetWeeklyProgressQuery : BaseStudentDashboardQueryModel, IRequest<MethodResult<IList<WeeklyProgressModel>>>
    {
    }
}
