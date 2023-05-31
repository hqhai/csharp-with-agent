// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonByCourseQuery : IRequest<MethodResult<object>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetListLessonByCourseQueryHandler : IRequestHandler<GetListLessonByCourseQuery, MethodResult<object>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitRepository _unitRepository;

        public GetListLessonByCourseQueryHandler(ILessonRepository lessonRepository, IUnitRepository unitRepository)
        {
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<object>> Handle(GetListLessonByCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            var unitQuery = await _unitRepository.Queryable
                                                     .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                                                     .ThenInclude(x => x.Lesson)
                                                     .Include(x => x.CourseUnitMockTests.Where(x => x.CourseId == request.CourseId))
                                                     .Select(x => new
                                                     {
                                                         Id = x.Id,
                                                         Name = x.Name,
                                                         Code = x.Code,
                                                         Lessons = x.UnitLessons.Select(x => x.Lesson).Select(x => new
                                                         {
                                                             Id = x!.Id,
                                                             Name = x.Name,
                                                         }).ToList(),
                                                     }).ToListAsync(cancellationToken);
            if (unitQuery.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitsNotExist));
                return methodResult;
            }
            methodResult.Result = unitQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
