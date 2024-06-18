// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.IntegrationQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class IntegrationLessonResultsQuery : IRequest<MethodResult<IList<LessonIntegration>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class IntegrationLessonResultsQueryHandler : IRequestHandler<IntegrationLessonResultsQuery, MethodResult<IList<LessonIntegration>>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public IntegrationLessonResultsQueryHandler(ILessonResultRepository lessonResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }
        public async Task<MethodResult<IList<LessonIntegration>>> Handle(IntegrationLessonResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonIntegration>>();

            var lessonQuerys = await _lessonResultRepository.Queryable
                                                            .Include(x => x.Lesson)
                                                            .Where(x => x.UpdatedDate == null ? (x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date) : (x.UpdatedDate.Value.Date >= request.StartDate.Date && x.UpdatedDate.Value.Date <= request.EndDate.Date))
                                                            .GroupBy(x => x.CreatedUserId)
                                                            .Select(x => new LessonIntegration
                                                            {
                                                                UserId = x.Key,
                                                                CurrentLesson = x.Where(c => c.CreatedUserId == x.Key).FirstOrDefault(c => c.Status == EnumResultStatus.New || c.Status == EnumResultStatus.Process).Lesson.Name,
                                                                LessonCompleted = x.Where(c => c.CreatedUserId == x.Key).Where(x => x.Status == EnumResultStatus.Done).Count()
                                                            })
                                                            .ToListAsync(cancellationToken);

            methodResult.Result = lessonQuerys;
            return methodResult;
        }
    }

    public class LessonIntegration
    {
        public Guid UserId { get; set; }

        public string? CurrentLesson { get; set; }

        public int? LessonCompleted { get; set; }
    }
}
