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
                            .Include(x => x.UnitLessons)
                            .Where(x => x.UnitLessons.Select(x => x.UnitId).Contains(request.UnitId))
                            .Select(x => new LessonModel
                            {
                                CreatedDate = x.CreatedDate,
                                Number = x.UnitLessons.Select(x => x.DisplayOrder).FirstOrDefault(),
                            }).ApplySort(request).ToListAsync(cancellationToken);

            methodResult.Result = lessons;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
