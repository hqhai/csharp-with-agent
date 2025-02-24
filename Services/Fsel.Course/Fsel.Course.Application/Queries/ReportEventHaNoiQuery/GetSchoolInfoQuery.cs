// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ReportEventHaNoiQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolInfoQuery : IRequest<MethodResult<IList<SchoolInfoModel>>>
    {
        public int Type { get; set; } = 1;

        public IList<string>? GroupIds { get; set; }

        public IList<string>? DistrictIds { get; set; }

        public int Level { get; set; }
    }

    public class GetSchoolInfoQueryHandler : IRequestHandler<GetSchoolInfoQuery, MethodResult<IList<SchoolInfoModel>>>
    {
        private readonly CourseDbContext _courseDbContext;

        public GetSchoolInfoQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<IList<SchoolInfoModel>>> Handle(GetSchoolInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SchoolInfoModel>> methodResult = new MethodResult<IList<SchoolInfoModel>>();

            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;

            var schoolInfos = await _courseDbContext.Set<SchoolInfoModel>()
                                                    .FromSqlRaw("EXEC SchoolInfoQuery @Type, @DistrictIds, @GroupIds, @Level",
                                                        new SqlParameter("@Type", request.Type),
                                                        new SqlParameter("@DistrictIds", districtIdsParam),
                                                        new SqlParameter("@GroupIds", groupIdsParam),
                                                        new SqlParameter("@Level", request.Level))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            methodResult.Result = schoolInfos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
