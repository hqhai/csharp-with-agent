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

    public class LearningProgressQuery : IRequest<MethodResult<LearningProgressModel>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public object? DistrictIds { get; set; }

        public object? GroupIds { get; set; }

        public object? SchoolIds { get; set; }
    }

    public class LearningProgressQueryHandler : IRequestHandler<LearningProgressQuery, MethodResult<LearningProgressModel>>
    {
        private readonly CourseDbContext _courseDbContext;

        public LearningProgressQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<LearningProgressModel>> Handle(LearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LearningProgressModel> methodResult = new MethodResult<LearningProgressModel>();

            var checkByGroup = 0;

            var total = await _courseDbContext.Set<TotalLearningProgressModel>()
                                              .FromSqlRaw("EXEC TotalLearningProgress @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                  new SqlParameter("@Target", request.Target),
                                                  new SqlParameter("@GroupByType", request.GroupByType),
                                                  new SqlParameter("@DistrictIds", request.DistrictIds ?? (object)DBNull.Value),
                                                  new SqlParameter("@GroupIds", request.GroupIds ?? (object)DBNull.Value),
                                                  new SqlParameter("@SchoolIds", request.SchoolIds ?? (object)DBNull.Value),
                                                  new SqlParameter("@CheckByGroup", checkByGroup))
                                              .AsNoTracking()
                                              .ToListAsync(cancellationToken);

            var average = await _courseDbContext.Set<AverageLearningProgressModel>()
                                                .FromSqlRaw("EXEC AverageLearningProgress @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                    new SqlParameter("@Target", request.Target),
                                                    new SqlParameter("@GroupByType", request.GroupByType),
                                                    new SqlParameter("@DistrictIds", request.DistrictIds ?? (object)DBNull.Value),
                                                    new SqlParameter("@GroupIds", request.GroupIds ?? (object)DBNull.Value),
                                                    new SqlParameter("@SchoolIds", request.SchoolIds ?? (object)DBNull.Value),
                                                    new SqlParameter("@CheckByGroup", checkByGroup))
                                                .AsNoTracking()
                                                .ToListAsync(cancellationToken);

            var unitDone = await _courseDbContext.Set<UnitDoneLearningProgressModel>()
                                                 .FromSqlRaw("EXEC UnitDoneLearningProgress @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                    new SqlParameter("@Target", request.Target),
                                                    new SqlParameter("@GroupByType", request.GroupByType),
                                                    new SqlParameter("@DistrictIds", request.DistrictIds ?? (object)DBNull.Value),
                                                    new SqlParameter("@GroupIds", request.GroupIds ?? (object)DBNull.Value),
                                                    new SqlParameter("@SchoolIds", request.SchoolIds ?? (object)DBNull.Value),
                                                    new SqlParameter("@CheckByGroup", checkByGroup))
                                                 .AsNoTracking()
                                                 .ToListAsync(cancellationToken);

            var lessonDone = await _courseDbContext.Set<LessonDoneLearningProgressModel>()
                                                   .FromSqlRaw("EXEC LessonDoneLearningProgress @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                        new SqlParameter("@Target", request.Target),
                                                        new SqlParameter("@GroupByType", request.GroupByType),
                                                        new SqlParameter("@DistrictIds", request.DistrictIds ?? (object)DBNull.Value),
                                                        new SqlParameter("@GroupIds", request.GroupIds ?? (object)DBNull.Value),
                                                        new SqlParameter("@SchoolIds", request.SchoolIds ?? (object)DBNull.Value),
                                                        new SqlParameter("@CheckByGroup", checkByGroup))
                                                   .AsNoTracking()
                                                   .ToListAsync(cancellationToken);

            methodResult.Result = new LearningProgressModel
            {
                TotalLearningProgress = total,
                AverageLearningProgress = average,
                UnitDoneLearningProgress = unitDone,
                LessonDoneLearningProgress = lessonDone
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
