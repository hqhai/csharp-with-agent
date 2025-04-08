// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.V1i1.ReportEvent
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class SchoolSummaryQuery : IRequest<MethodResult<PagingItemsModel<SchoolSummaryModel>>>
    {
        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GradeLevels { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public int? TargetGroup { get; set; }

        public DateTime? Date { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public class SchoolSummaryQueryHandler : IRequestHandler<SchoolSummaryQuery, MethodResult<PagingItemsModel<SchoolSummaryModel>>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<PagingItemsModel<SchoolSummaryModel>> _cacheService;
        private readonly AuthContext _authContext;

        public SchoolSummaryQueryHandler(CourseDbContext courseDbContext,
                                         ICacheService<PagingItemsModel<SchoolSummaryModel>> cacheService,
                                         AuthContext authContext)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolSummaryModel>>> Handle(SchoolSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SchoolSummaryModel>>();

            var keyCache = $"SchoolSummaryReport_{ConvertHelper.Serialize(request)}";
            var data = await _cacheService.GetAsync(keyCache);
            if (data != null)
            {
                methodResult.Result = data;
                return methodResult;
            }

            StringBuilder sbDistrict = new StringBuilder();
            if (request.DistrictIds != null)
            {
                foreach (var id in request.DistrictIds)
                {
                    if (sbDistrict.Length > 0)
                    {
                        sbDistrict.Append(",");
                    }

                    sbDistrict.Append(id);
                }
            }
            var districtIdsParam = sbDistrict.Length > 0 ? sbDistrict.ToString() : (object)DBNull.Value;

            StringBuilder sbGradeLevel = new StringBuilder();
            if (request.GradeLevels != null)
            {
                foreach (var gradeLevel in request.GradeLevels)
                {
                    if (sbGradeLevel.Length > 0)
                    {
                        sbGradeLevel.Append(",");
                    }

                    sbGradeLevel.Append(gradeLevel);
                }
            }
            var gradeLevelsParam = sbGradeLevel.Length > 0 ? sbGradeLevel.ToString() : (object)DBNull.Value;

            StringBuilder sbSchool = new StringBuilder();
            if (request.SchoolIds != null)
            {
                foreach (var id in request.SchoolIds)
                {
                    if (sbSchool.Length > 0)
                    {
                        sbSchool.Append(",");
                    }

                    sbSchool.Append(id);
                }
            }
            var schoolIdsParam = sbSchool.Length > 0 ? sbSchool.ToString() : (object)DBNull.Value;

            var date = request.Date != null ? request.Date : (object)DBNull.Value;

            var result = await _courseDbContext.Set<SchoolSummaryModel>()
                                               .FromSqlRaw("EXEC SchoolSummaryReport  @UserId, @DistrictIds, @GradeLevel, @SchoolIds, @TargetGroup, @Date, @PageNumber, @PageSize",
                                                   new SqlParameter("@UserId", _authContext.CurrentUserId),
                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                   new SqlParameter("@GradeLevel", gradeLevelsParam),
                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                   new SqlParameter("@TargetGroup", request.TargetGroup),
                                                   new SqlParameter("@Date", date),
                                                   new SqlParameter("@PageNumber", request.PageNumber),
                                                   new SqlParameter("@PageSize", request.PageSize))
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);

            var dataResult = new PagingItemsModel<SchoolSummaryModel>
            {
                Items = result,
                PagingInfo = new PagingInfoModel { Page = request.PageNumber, PageSize = request.PageSize, TotalItems = result.FirstOrDefault()?.TotalItem ?? default }
            };

            methodResult.Result = dataResult;
            await _cacheService.SetAsync(keyCache, dataResult, TimeSpan.FromSeconds(CacheSettings.TimeCache.ThreeHour));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
