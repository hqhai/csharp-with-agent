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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class IntegrationUnitResultsQueryHandler : IRequestHandler<IntegrationUnitResultsQuery, MethodResult<IList<UnitResultIntegration>>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public IntegrationUnitResultsQueryHandler(IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }
        public async Task<MethodResult<IList<UnitResultIntegration>>> Handle(IntegrationUnitResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UnitResultIntegration>>();

            var lessonResults = await _lessonResultRepository.Queryable
                                                             .Include(x => x.Lesson)
                                                             .Where(x => (x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) :
                                                                                            (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate)) &&
                                                                                            (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                             .ToListAsync(cancellationToken);

            IList<UnitResultIntegration> unitResults = new List<UnitResultIntegration>();

            var userIds = lessonResults.Select(x => x.CreatedUserId).Distinct().ToList();

            var unitQuerys = await _unitResultRepository.Queryable
                                                        .Include(x => x.Unit)
                                                        .Where(x => userIds.Contains(x.CreatedUserId) && (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                        .ToListAsync(cancellationToken);

            foreach (var item in unitQuerys)
            {
                var unitResult = new UnitResultIntegration
                {
                    UserId = item.CreatedUserId,
                    Name = item.Unit?.Name,
                    CurrentLesson = lessonResults.FirstOrDefault(x => x.CreatedUserId == item.CreatedUserId)?.Lesson?.Name,
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
    }
}
