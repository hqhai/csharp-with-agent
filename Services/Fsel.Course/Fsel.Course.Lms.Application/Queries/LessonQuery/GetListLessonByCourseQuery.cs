// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonByCourseQuery : IRequest<MethodResult<object>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetListLessonByCourseQueryHandler : IRequestHandler<GetListLessonByCourseQuery, MethodResult<object>>
    {
        private readonly IUnitRepository _unitRepository;

        public GetListLessonByCourseQueryHandler(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<object>> Handle(GetListLessonByCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            var unitQuery = await _unitRepository.Queryable
                                                     .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                                                     .ThenInclude(x => x.Lesson)
                                                     .Include(x => x.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
                                                     .Where(x => x.CourseUnitMockTests.Select(x => x.CourseId).Contains(request.CourseId))
                                                     .OrderBy(x => x.CourseUnitMockTests.Select(x => x.Number).FirstOrDefault())
                                                     .AsNoTracking()
                                                     .Select(x => new UnitModel
                                                     {
                                                         Id = x.Id,
                                                         Name = x.Name,
                                                         Code = x.Code,
                                                         Lessons = x.UnitLessons.OrderBy(x => x.DisplayOrder).Select(x => x.Lesson).Select(x => new LessonModel
                                                         {
                                                             Id = x!.Id,
                                                             Name = x.Name,
                                                         }).ToList(),
                                                     }).ToListAsync(cancellationToken);

            methodResult.Result = unitQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
