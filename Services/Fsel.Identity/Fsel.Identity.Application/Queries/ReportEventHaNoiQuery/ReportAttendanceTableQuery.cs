// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ReportEventHaNoiQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Caching;
    using Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Identity.Infrastructure;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class ReportAttendanceTableQuery : IRequest<MethodResult<IList<SummaryDataOnCityModel>>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ReportAttendanceTableQueryHandler : IRequestHandler<ReportAttendanceTableQuery, MethodResult<IList<SummaryDataOnCityModel>>>
    {
        private readonly UserDbContext _userDbContext;
        private readonly ICacheService<IList<SummaryDataOnCityModel>> _cacheService;

        public ReportAttendanceTableQueryHandler(UserDbContext userDbContext, ICacheService<IList<SummaryDataOnCityModel>> cacheService)
        {
            _userDbContext = userDbContext;
            _cacheService = cacheService;
        }

        public async Task<MethodResult<IList<SummaryDataOnCityModel>>> Handle(ReportAttendanceTableQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SummaryDataOnCityModel>>();

            var keyCache = $"SummaryDataOnCity_{ConvertHelper.Serialize(request)}";
            var data = await _cacheService.GetAsync(keyCache);
            if (data != null)
            {
                methodResult.Result = data;
                return methodResult;
            }

            var checkByGroup = 0;
            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;

            var summaryDataOnCity = await _userDbContext.Set<SummaryDataOnCityModel>()
                                                        .FromSqlRaw("EXEC SummaryDataOnCity @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date, @CheckByGroup",
                                                            new SqlParameter("@Target", request.Target),
                                                            new SqlParameter("@GroupByType", request.GroupByType),
                                                            new SqlParameter("@DistrictIds", districtIdsParam),
                                                            new SqlParameter("@GroupIds", groupIdsParam),
                                                            new SqlParameter("@SchoolIds", schoolIdsParam),
                                                            new SqlParameter("@Date", request.Date ?? DateTime.UtcNow), // Truyền đúng tham số Date
                                                            new SqlParameter("@CheckByGroup", checkByGroup))
                                                        .AsNoTracking()
                                                        .ToListAsync(cancellationToken);


            methodResult.Result = summaryDataOnCity;
            await _cacheService.SetAsync(keyCache, summaryDataOnCity, TimeSpan.FromSeconds(CacheSettings.TimeCache.ThreeHour));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
