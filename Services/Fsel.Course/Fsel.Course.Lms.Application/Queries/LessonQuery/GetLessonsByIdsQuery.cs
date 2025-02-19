// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonsByIdsQuery : IRequest<MethodResult<IList<LessonModel>>>
    {
        public IList<Guid>? LessonIds { get; set; }
    }

    public class GetLessonsByIdsQueryHandler : IRequestHandler<GetLessonsByIdsQuery, MethodResult<IList<LessonModel>>>
    {
        private readonly ILessonRepository _lessonRepository;

        public GetLessonsByIdsQueryHandler(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(GetLessonsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonModel>> methodResult = new MethodResult<IList<LessonModel>>();

            if (request.LessonIds == null || request.LessonIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonIds));
                return methodResult;
            }

            var lessons = await _lessonRepository.Queryable
                                                 .WhereBulkContains(request.LessonIds, p => p.Id)
                                                 .Select(x => new LessonModel
                                                 {
                                                     Id = x.Id,
                                                     Name = x.Name,
                                                     CourseLevel = x.CourseLevel,
                                                     InstructionContent = x.InstructionContent,
                                                     IsActive = x.IsArchive,
                                                 }).ToListAsync(cancellationToken);

            methodResult.Result = lessons;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
