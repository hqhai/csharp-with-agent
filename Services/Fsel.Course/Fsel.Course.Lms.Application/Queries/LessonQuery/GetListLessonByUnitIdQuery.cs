// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonByUnitIdQuery : BaseQueryModel, IRequest<MethodResult<IList<LessonModel>>>
    {
        public Guid UnitId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetListLessonByUnitIdQueryHandler : IRequestHandler<GetListLessonByUnitIdQuery, MethodResult<IList<LessonModel>>>
    {
        private readonly ILessonRepository _lessonRepository;

        public GetListLessonByUnitIdQueryHandler(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(GetListLessonByUnitIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonModel>>();

            var lessons = await _lessonRepository.Queryable
                            .Include(x => x.LessonResults)
                            .Include(x => x.UnitLessons)
                            .ThenInclude(x => x.Unit)
                            .ThenInclude(x => x.CourseUnitMockTests)
                            .Where(x => x.LessonResults.Select(x => x.UnitId).FirstOrDefault() == request.UnitId && x.LessonResults.Select(x => x.Unit).SelectMany(x => x.CourseUnitMockTests).Select(x => x.CourseId).FirstOrDefault() == request.CourseId)
                            .Select(x => new LessonModel
                            {
                                CreatedDate = x.CreatedDate,
                                Number = x.LessonResults.Select(x => x.Unit).SelectMany(x => x.CourseUnitMockTests).Select(x => x.Number).FirstOrDefault(),
                            }).ApplySort(request).ToListAsync(cancellationToken);

            methodResult.Result = lessons;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
