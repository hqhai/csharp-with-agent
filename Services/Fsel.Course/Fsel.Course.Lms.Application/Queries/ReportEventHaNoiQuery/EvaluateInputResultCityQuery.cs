// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportEventHaNoiQuery
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

    public class EvaluateInputResultCityQuery : IRequest<MethodResult<EvaluateInputResultModel>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }
    }

    public class EvaluateInputResultCityQueryHandler : IRequestHandler<EvaluateInputResultCityQuery, MethodResult<EvaluateInputResultModel>>
    {
        private readonly CourseDbContext _courseDbContext;

        public EvaluateInputResultCityQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<EvaluateInputResultModel>> Handle(EvaluateInputResultCityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<EvaluateInputResultModel> methodResult = new MethodResult<EvaluateInputResultModel>();

            var checkByGroup = 0;
            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;

            var total = await _courseDbContext.Set<TotalEvaluateInputResultModel>()
                                               .FromSqlRaw("EXEC TotalEvaluateInputResults @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                   new SqlParameter("@Target", request.Target),
                                                   new SqlParameter("@GroupByType", request.GroupByType),
                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                   new SqlParameter("@GroupIds", groupIdsParam),
                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                   new SqlParameter("@CheckByGroup", checkByGroup))
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);

            var totalDetail = await _courseDbContext.Set<TotalDetailEvaluateInputResultModel>()
                                                    .FromSqlRaw("EXEC TotalDetailEvaluateInputResults @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                        new SqlParameter("@Target", request.Target),
                                                        new SqlParameter("@GroupByType", request.GroupByType),
                                                        new SqlParameter("@DistrictIds", districtIdsParam),
                                                        new SqlParameter("@GroupIds", groupIdsParam),
                                                        new SqlParameter("@SchoolIds", schoolIdsParam),
                                                        new SqlParameter("@CheckByGroup", checkByGroup))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            var percent = await _courseDbContext.Set<PercentEvaluateInputResultModel>()
                                                .FromSqlRaw("EXEC PercentEvaluateInputResults @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                   new SqlParameter("@Target", request.Target),
                                                   new SqlParameter("@GroupByType", request.GroupByType),
                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                   new SqlParameter("@GroupIds", groupIdsParam),
                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                   new SqlParameter("@CheckByGroup", checkByGroup))
                                                .AsNoTracking()
                                                .ToListAsync(cancellationToken);

            var level = await _courseDbContext.Set<LevelEvaluateInputResultModel>()
                                              .FromSqlRaw("EXEC LevelEvaluateInputResults @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                   new SqlParameter("@Target", request.Target),
                                                   new SqlParameter("@GroupByType", request.GroupByType),
                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                   new SqlParameter("@GroupIds", groupIdsParam),
                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                   new SqlParameter("@CheckByGroup", checkByGroup))
                                              .AsNoTracking()
                                              .ToListAsync(cancellationToken);

            methodResult.Result = new EvaluateInputResultModel
            {
                TotalEvaluateInputResults = total,
                TotalDetailEvaluateInputResults = totalDetail,
                PercentEvaluateInputResults = percent,
                LevelEvaluateInputResults = level
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
