// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.V1i1.ReportEvent
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class GetDistrictInfoQuery : IRequest<MethodResult<IList<DistrictInfoModel>>>
    {
    }

    public class GetDistrictInfoQueryHandler : IRequestHandler<GetDistrictInfoQuery, MethodResult<IList<DistrictInfoModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly CourseDbContext _courseDbContext;

        public GetDistrictInfoQueryHandler(AuthContext authContext,
                                           CourseDbContext courseDbContext)
        {
            _authContext = authContext;
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<IList<DistrictInfoModel>>> Handle(GetDistrictInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<DistrictInfoModel>> methodResult = new MethodResult<IList<DistrictInfoModel>>();

            var totalDetail = await _courseDbContext.Set<DistrictInfoModel>()
                                                    .FromSqlRaw("EXEC DistrictInfo @UserId",
                                                        new SqlParameter("@UserId", _authContext.CurrentUserId))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            methodResult.Result = totalDetail;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
