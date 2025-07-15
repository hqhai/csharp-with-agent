// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ReportEventHaNoiQuery
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class TotalLearningQualityChartQuery : IRequest<MethodResult<IList<TotalLearningQualityModel>>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public DateTime? Date { get; set; }
    }

    public class TotalLearningQualityChartQueryHandler : IRequestHandler<TotalLearningQualityChartQuery, MethodResult<IList<TotalLearningQualityModel>>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<IList<TotalLearningQualityModel>> _cacheService;
        private readonly AppSetting _appSetting;

        public TotalLearningQualityChartQueryHandler(CourseDbContext courseDbContext,
                                                     ICacheService<IList<TotalLearningQualityModel>> cacheService,
                                                     AppSetting appSetting)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<IList<TotalLearningQualityModel>>> Handle(TotalLearningQualityChartQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TotalLearningQualityModel>>();

            var keyCache = $"TotalLearningQuality_{ConvertHelper.Serialize(request)}";
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

            StringBuilder sbGroup = new StringBuilder();
            if (request.GroupIds != null)
            {
                foreach (var id in request.GroupIds)
                {
                    if (sbGroup.Length > 0)
                    {
                        sbGroup.Append(",");
                    }

                    sbGroup.Append(id);
                }
            }
            var groupIdsParam = sbGroup.Length > 0 ? sbGroup.ToString() : (object)DBNull.Value;

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

            var learningQuality = await _courseDbContext.Set<TotalLearningQualityModel>()
                                                        .FromSqlRaw("EXEC TotalLearningQuality @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                           new SqlParameter("@Target", request.Target),
                                                           new SqlParameter("@GroupByType", request.GroupByType),
                                                           new SqlParameter("@DistrictIds", districtIdsParam),
                                                           new SqlParameter("@GroupIds", groupIdsParam),
                                                           new SqlParameter("@SchoolIds", schoolIdsParam),
                                                           new SqlParameter("@Date", date))
                                                        .AsNoTracking()
                                                        .ToListAsync(cancellationToken);

            methodResult.Result = learningQuality;
            if (_appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                await _cacheService.SetAsync(keyCache, learningQuality, TimeSpan.FromSeconds(_appSetting.CacheConfig.CachingDuration));
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
