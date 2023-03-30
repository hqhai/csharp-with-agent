// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonQuery : IRequest<MethodResult<IList<LessonModel>>>
    {
        public Guid UnitId { get; set; }
    }

    public class GetListLessonQueryHandler : IRequestHandler<GetListLessonQuery, MethodResult<IList<LessonModel>>>
    {
        private readonly ILessonRepository _lessonRepository;

        public GetListLessonQueryHandler(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(GetListLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonModel>> methodResult = new MethodResult<IList<LessonModel>>();
            var lessonQuery = await _lessonRepository.Queryable
                                .Include(x => x.UnitLessons)
                                .Select(x => new LessonModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    InstructionContent = x.InstructionContent,
                                    DisplayName = x.DisplayName,
                                }).ToListAsync(cancellationToken: cancellationToken);

            if (lessonQuery.Count == 0)
            {
                methodResult.AddErrorBadRequest(
                  nameof(EnumLessonErrorCode.ListLessonNotExist));
                return methodResult;
            }
            methodResult.Result = lessonQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
