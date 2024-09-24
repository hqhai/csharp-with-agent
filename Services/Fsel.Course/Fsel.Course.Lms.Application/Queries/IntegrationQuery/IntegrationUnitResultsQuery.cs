// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.IntegrationQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class IntegrationUnitResultsQuery : IRequest<MethodResult<IList<UnitResultIntegration>>>
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }

    public class IntegrationUnitResultsQueryHandler : IRequestHandler<IntegrationUnitResultsQuery, MethodResult<IList<UnitResultIntegration>>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public IntegrationUnitResultsQueryHandler(IUnitResultRepository unitResultRepository,
                                                  ILessonResultRepository lessonResultRepository,
                                                  ICourseResultRepository courseResultRepository)
        {
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
        }
        public async Task<MethodResult<IList<UnitResultIntegration>>> Handle(IntegrationUnitResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UnitResultIntegration>>();

            if (request.UserIds == null)
            {
                var lessonResultHasTimes = await _lessonResultRepository.Queryable
                                                                        .Include(x => x.Lesson)
                                                                        .Where(x => (x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) :
                                                                              (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate)) &&
                                                                              (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                                        .ToListAsync(cancellationToken);

                var datas = lessonResultHasTimes.GroupBy(x => x.CreatedUserId)
                                                .Select(x => new UnitResultIntegration
                                                {
                                                    UserId = x.Key
                                                }).ToList();

                methodResult.Result = datas;
                return methodResult;
            }

            var lessonResults = await _lessonResultRepository.Queryable
                                                 .Include(x => x.Lesson)
                                                 .Where(x => request.UserIds.Contains(x.CreatedUserId) &&
                                                       (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                 .ToListAsync(cancellationToken);

            IList<UnitResultIntegration> unitResults = new List<UnitResultIntegration>();

            var userIds = lessonResults.Select(x => x.CreatedUserId).Distinct().ToList();

            var unitQuerys = await _unitResultRepository.Queryable
                                                        .Include(x => x.Unit)
                                                        .Include(x => x.Course)
                                                        .Where(x => userIds.Contains(x.CreatedUserId) && (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                        .ToListAsync(cancellationToken);

            var courseIds = unitQuerys.Select(x => x.CourseId).Distinct().ToList();

            var courseResults = await _courseResultRepository.Queryable.Where(x => courseIds.Contains(x.CourseId) && x.Status != EnumResultStatus.Unfinished && userIds.Contains(x.CreatedUserId)).ToListAsync(cancellationToken);

            foreach (var item in unitQuerys)
            {
                var unitResult = new UnitResultIntegration
                {
                    UserId = item.CreatedUserId,
                    Name = item.Unit?.Name,
                    StartCourse = courseResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId && x.CourseId == item.CourseId)?.CreatedDate,
                    EndCourse = courseResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId && x.CourseId == item.CourseId && x.Status == EnumResultStatus.Done)?.UpdatedDate,
                    CourseLevel = item.Course?.CourseLevel.ToString(),
                    CurrentLesson = lessonResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId)?.Lesson?.Name,
                    DateEdit = lessonResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId)?.UpdatedDate != null ? lessonResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId)?.UpdatedDate : lessonResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId)?.CreatedDate,
                    LessonCompleted = await _lessonResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.UnitId == item.UnitId && x.CreatedUserId == item.CreatedUserId).CountAsync(cancellationToken)
                };

                unitResults.Add(unitResult);
            }

            methodResult.Result = unitResults;
            return methodResult;
        }
    }

    public class UnitResultIntegration
    {
        public Guid UserId { get; set; }

        public string? Name { get; set; }

        public string? CurrentLesson { get; set; }

        public int? LessonCompleted { get; set; }

        public string? CourseLevel { get; set; }

        public DateTime? StartCourse { get; set; }

        public DateTime? EndCourse { get; set; }

        public DateTime? DateEdit { get; set; }
    }
}
