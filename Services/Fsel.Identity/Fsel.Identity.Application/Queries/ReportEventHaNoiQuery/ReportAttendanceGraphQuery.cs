// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ReportEventHaNoiQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Identity.Infrastructure;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class ReportAttendanceGraphQuery : IRequest<MethodResult<IList<NumberStudentLearnOnSystemModel>>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }
        public DateTime? Date { get; set; }
        public bool? SortByHour { get; set; }
    }

    public class ReportAttendanceGraphQueryHandler : IRequestHandler<ReportAttendanceGraphQuery, MethodResult<IList<NumberStudentLearnOnSystemModel>>>
    {
        private readonly UserDbContext _userDbContext;
        private readonly ICacheService<IList<NumberStudentLearnOnSystemModel>> _cacheService;

        public ReportAttendanceGraphQueryHandler(UserDbContext userDbContext, ICacheService<IList<NumberStudentLearnOnSystemModel>> cacheService)
        {
            _userDbContext = userDbContext;
            _cacheService = cacheService;
        }

        public async Task<MethodResult<IList<NumberStudentLearnOnSystemModel>>> Handle(ReportAttendanceGraphQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<NumberStudentLearnOnSystemModel>>();

            var keyCache = ConvertHelper.Serialize(request);
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

            var numberStudentLearnOnSystem = await _userDbContext.Set<NumberStudentLearnOnSystemModel>()
                                                                 .FromSqlRaw("EXEC NumberStudentLearnOnSystem @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date, @CheckByGroup, @SortByHour",
                                                                     new SqlParameter("@Target", request.Target),
                                                                     new SqlParameter("@GroupByType", request.GroupByType),
                                                                     new SqlParameter("@DistrictIds", districtIdsParam),
                                                                     new SqlParameter("@GroupIds", groupIdsParam),
                                                                     new SqlParameter("@SchoolIds", schoolIdsParam),
                                                                     new SqlParameter("@Date", request.Date ?? DateTime.UtcNow),
                                                                     new SqlParameter("@CheckByGroup", checkByGroup),
                                                                     new SqlParameter("@SortByHour", request.SortByHour))
                                                                 .AsNoTracking()
                                                                 .ToListAsync(cancellationToken);

            methodResult.Result = numberStudentLearnOnSystem;
            await _cacheService.SetAsync(keyCache, numberStudentLearnOnSystem, TimeSpan.FromSeconds(CacheSettings.TimeCache.ThreeHour));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
