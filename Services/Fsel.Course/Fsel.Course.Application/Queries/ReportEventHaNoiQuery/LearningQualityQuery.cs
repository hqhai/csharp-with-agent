// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ReportEventHaNoiQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class LearningQualityQuery : IRequest<MethodResult<LearningQualityModel>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public DateTime? Date { get; set; }
    }

    public class LearningQualityQueryHandler : IRequestHandler<LearningQualityQuery, MethodResult<LearningQualityModel>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<LearningQualityModel> _cacheService;
        private const string KeyCache = "LearningQuality";

        public LearningQualityQueryHandler(CourseDbContext courseDbContext, ICacheService<LearningQualityModel> cacheService)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
        }

        public async Task<MethodResult<LearningQualityModel>> Handle(LearningQualityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LearningQualityModel>();

            //var data = await _cacheService.GetAsync(KeyCache);
            //if (data != null)
            //{
            //    methodResult.Result = data;
            //    return methodResult;
            //}

            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;
            var date = request.Date != null ? request.Date : (object)DBNull.Value;

            var total = await _courseDbContext.Set<TotalLearningModel>()
                                              .FromSqlRaw("EXEC TotalLearningResults @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                  new SqlParameter("@Target", request.Target),
                                                  new SqlParameter("@GroupByType", request.GroupByType),
                                                  new SqlParameter("@DistrictIds", districtIdsParam),
                                                  new SqlParameter("@GroupIds", groupIdsParam),
                                                  new SqlParameter("@SchoolIds", schoolIdsParam),
                                                  new SqlParameter("@Date", date))
                                              .AsNoTracking()
                                              .ToListAsync(cancellationToken);

            var rate = await _courseDbContext.Set<RateLearningModel>()
                                             .FromSqlRaw("EXEC RateLearningResults @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                 new SqlParameter("@Target", request.Target),
                                                 new SqlParameter("@GroupByType", request.GroupByType),
                                                 new SqlParameter("@DistrictIds", districtIdsParam),
                                                 new SqlParameter("@GroupIds", groupIdsParam),
                                                 new SqlParameter("@SchoolIds", schoolIdsParam),
                                                 new SqlParameter("@Date", date))
                                             .AsNoTracking()
                                             .ToListAsync(cancellationToken);

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

            var detailLearningQuality = await _courseDbContext.Set<TotalDetailLearningQualityModel>()
                                                              .FromSqlRaw("EXEC TotalDetailLearningQuality @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                                   new SqlParameter("@Target", request.Target),
                                                                   new SqlParameter("@GroupByType", request.GroupByType),
                                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                                   new SqlParameter("@GroupIds", groupIdsParam),
                                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                                   new SqlParameter("@Date", date))
                                                              .AsNoTracking()
                                                              .ToListAsync(cancellationToken);

            var setDataCache = new LearningQualityModel
            {
                TotalLearnings = total,
                RateLearnings = rate,
                TotalLearningQualitys = learningQuality,
                TotalDetailLearningQualitys = detailLearningQuality
            };

            methodResult.Result = setDataCache;
            //await _cacheService.SetAsync(KeyCache, setDataCache, TimeSpan.FromSeconds(CacheSettings.TimeCache.OneHour));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
