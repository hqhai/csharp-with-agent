// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ReportEventHaNoiQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Caching;
    using Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Identity.Infrastructure;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class ReportAttendanceForCityQuery : IRequest<MethodResult<OverallStudentModel>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ReportAttendanceForCityQueryHandler : IRequestHandler<ReportAttendanceForCityQuery, MethodResult<OverallStudentModel>>
    {
        private readonly UserDbContext _userDbContext;
        private readonly ICacheService<OverallStudentModel> _cacheService;
        private readonly AppSetting _appSetting;

        public ReportAttendanceForCityQueryHandler(UserDbContext userDbContext,
                                                   ICacheService<OverallStudentModel> cacheService,
                                                   AppSetting appSetting)
        {
            _userDbContext = userDbContext;
            _cacheService = cacheService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<OverallStudentModel>> Handle(ReportAttendanceForCityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallStudentModel>();

            var keyCache = $"AttendanceReport_{ConvertHelper.Serialize(request)}";
            var data = await _cacheService.GetAsync(keyCache);
            if (data != null && _appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                methodResult.Result = data;
                return methodResult;
            }

            var checkByGroup = 0;
            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;

            var totalList = await _userDbContext.Set<OverallStudentModel>()
                                                .FromSqlRaw("EXEC AttendanceReport @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date, @CheckByGroup",
                                                    new SqlParameter("@Target", request.Target),
                                                    new SqlParameter("@GroupByType", request.GroupByType),
                                                    new SqlParameter("@DistrictIds", districtIdsParam),
                                                    new SqlParameter("@GroupIds", groupIdsParam),
                                                    new SqlParameter("@SchoolIds", schoolIdsParam),
                                                    new SqlParameter("@Date", request.Date ?? DateTime.UtcNow), // Truyền đúng tham số Date
                                                    new SqlParameter("@CheckByGroup", checkByGroup))
                                                .AsNoTracking()
                                                .ToListAsync(cancellationToken);


            var overallStudent = totalList.FirstOrDefault();

            methodResult.Result = overallStudent;
            if (overallStudent != null && _appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                await _cacheService.SetAsync(keyCache, overallStudent, TimeSpan.FromSeconds(_appSetting.CacheConfig.CachingDuration));
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
