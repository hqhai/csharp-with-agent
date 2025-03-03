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

    public class LearningProgressQuery : IRequest<MethodResult<LearningProgressModel>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public DateTime? Date { get; set; }
    }

    public class LearningProgressQueryHandler : IRequestHandler<LearningProgressQuery, MethodResult<LearningProgressModel>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<LearningProgressModel> _cacheService;
        private const string KeyCache = "LearningProgress";

        public LearningProgressQueryHandler(CourseDbContext courseDbContext, ICacheService<LearningProgressModel> cacheService)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
        }

        public async Task<MethodResult<LearningProgressModel>> Handle(LearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LearningProgressModel>();

            var data = await _cacheService.GetAsync(KeyCache);
            if (data != null)
            {
                methodResult.Result = data;
                return methodResult;
            }

            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;
            var date = request.Date != null ? request.Date : (object)DBNull.Value;

            var total = await _courseDbContext.Set<TotalLearningProgressModel>()
                                              .FromSqlRaw("EXEC TotalLearningProgress @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                  new SqlParameter("@Target", request.Target),
                                                  new SqlParameter("@GroupByType", request.GroupByType),
                                                  new SqlParameter("@DistrictIds", districtIdsParam),
                                                  new SqlParameter("@GroupIds", groupIdsParam),
                                                  new SqlParameter("@SchoolIds", schoolIdsParam),
                                                  new SqlParameter("@Date", date))
                                              .AsNoTracking()
                                              .ToListAsync(cancellationToken);

            var average = await _courseDbContext.Set<AverageLearningProgressModel>()
                                                .FromSqlRaw("EXEC AverageLearningProgress @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                    new SqlParameter("@Target", request.Target),
                                                    new SqlParameter("@GroupByType", request.GroupByType),
                                                    new SqlParameter("@DistrictIds", districtIdsParam),
                                                    new SqlParameter("@GroupIds", groupIdsParam),
                                                    new SqlParameter("@SchoolIds", schoolIdsParam),
                                                  new SqlParameter("@Date", date))
                                                .AsNoTracking()
                                                .ToListAsync(cancellationToken);

            var unitDoneAca = await _courseDbContext.Set<UnitDoneLearningProgressAcademicModel>()
                                                    .FromSqlRaw("EXEC UnitDoneLearningProgressAcademic @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                       new SqlParameter("@Target", request.Target),
                                                       new SqlParameter("@GroupByType", request.GroupByType),
                                                       new SqlParameter("@DistrictIds", districtIdsParam),
                                                       new SqlParameter("@GroupIds", groupIdsParam),
                                                       new SqlParameter("@SchoolIds", schoolIdsParam),
                                                       new SqlParameter("@Date", date))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            var lessonDoneAca = await _courseDbContext.Set<LessonDoneLearningProgressAcademicModel>()
                                                      .FromSqlRaw("EXEC LessonDoneLearningProgressAcademic @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                           new SqlParameter("@Target", request.Target),
                                                           new SqlParameter("@GroupByType", request.GroupByType),
                                                           new SqlParameter("@DistrictIds", districtIdsParam),
                                                           new SqlParameter("@GroupIds", groupIdsParam),
                                                           new SqlParameter("@SchoolIds", schoolIdsParam),
                                                           new SqlParameter("@Date", date))
                                                      .AsNoTracking()
                                                      .ToListAsync(cancellationToken);

            var unitDoneIelts = await _courseDbContext.Set<UnitDoneLearningProgressIeltsModel>()
                                                      .FromSqlRaw("EXEC UnitDoneLearningProgressIelts @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                         new SqlParameter("@Target", request.Target),
                                                         new SqlParameter("@GroupByType", request.GroupByType),
                                                         new SqlParameter("@DistrictIds", districtIdsParam),
                                                         new SqlParameter("@GroupIds", groupIdsParam),
                                                         new SqlParameter("@SchoolIds", schoolIdsParam),
                                                         new SqlParameter("@Date", date))
                                                      .AsNoTracking()
                                                      .ToListAsync(cancellationToken);

            var lessonDoneIelts = await _courseDbContext.Set<LessonDoneLearningProgressIeltsModel>()
                                                        .FromSqlRaw("EXEC LessonDoneLearningProgressIelts @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                             new SqlParameter("@Target", request.Target),
                                                             new SqlParameter("@GroupByType", request.GroupByType),
                                                             new SqlParameter("@DistrictIds", districtIdsParam),
                                                             new SqlParameter("@GroupIds", groupIdsParam),
                                                             new SqlParameter("@SchoolIds", schoolIdsParam),
                                                             new SqlParameter("@Date", date))
                                                        .AsNoTracking()
                                                        .ToListAsync(cancellationToken);

            var setDataCache = new LearningProgressModel
            {
                TotalLearningProgress = total,
                AverageLearningProgress = average,
                UnitDoneLearningProgressAcademics = unitDoneAca,
                LessonDoneLearningProgressAcademics = lessonDoneAca,
                UnitDoneLearningProgressIelts = unitDoneIelts,
                LessonDoneLearningProgressIelts = lessonDoneIelts
            };

            methodResult.Result = setDataCache;
            await _cacheService.SetAsync(KeyCache, setDataCache, TimeSpan.FromSeconds(CacheSettings.TimeCache.OneHour));

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
