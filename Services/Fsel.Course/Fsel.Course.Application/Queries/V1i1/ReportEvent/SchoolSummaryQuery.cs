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
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Infrastructure.ValueSettings;
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

        public int CheckByGroup { get; set; }
    }

    public class SchoolSummaryQueryHandler : IRequestHandler<SchoolSummaryQuery, MethodResult<PagingItemsModel<SchoolSummaryModel>>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<PagingItemsModel<SchoolSummaryModel>> _cacheService;
        private readonly AuthContext _authContext;
        private readonly AppSetting _appSetting;

        public SchoolSummaryQueryHandler(CourseDbContext courseDbContext,
                                         ICacheService<PagingItemsModel<SchoolSummaryModel>> cacheService,
                                         AuthContext authContext,
                                         AppSetting appSetting)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
            _authContext = authContext;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolSummaryModel>>> Handle(SchoolSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SchoolSummaryModel>>();

            var keyCache = $"SchoolSummaryReport_{ConvertHelper.Serialize(request)}_{_authContext.CurrentUserId}";
            var data = await _cacheService.GetAsync(keyCache);
            if (data != null && _appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
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
                                               .FromSqlRaw("EXEC SchoolSummaryReport  @UserId, @DistrictIds, @GradeLevel, @SchoolIds, @TargetGroup, @Date, @PageNumber, @PageSize, @CheckByGroup",
                                                   new SqlParameter("@UserId", _authContext.CurrentUserId),
                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                   new SqlParameter("@GradeLevel", gradeLevelsParam),
                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                   new SqlParameter("@TargetGroup", request.TargetGroup),
                                                   new SqlParameter("@Date", date),
                                                   new SqlParameter("@PageNumber", request.PageNumber),
                                                   new SqlParameter("@PageSize", request.PageSize),
                                                   new SqlParameter("@CheckByGroup", request.CheckByGroup))
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);

            var dataResult = new PagingItemsModel<SchoolSummaryModel>
            {
                Items = result,
                PagingInfo = new PagingInfoModel { Page = request.PageNumber, PageSize = request.PageSize, TotalItems = result.FirstOrDefault()?.TotalItem ?? default }
            };

            methodResult.Result = dataResult;
            if (_appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                await _cacheService.SetAsync(keyCache, dataResult, TimeSpan.FromSeconds(_appSetting.CacheConfig.CachingDuration));
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
