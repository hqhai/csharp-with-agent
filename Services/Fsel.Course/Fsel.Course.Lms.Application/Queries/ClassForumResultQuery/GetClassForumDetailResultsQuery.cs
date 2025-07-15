// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumDetailResultsQuery : IRequest<MethodResult<IList<ClassForumDetailResultModel>>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetClassForumDetailResultsQueryHandler : IRequestHandler<GetClassForumDetailResultsQuery, MethodResult<IList<ClassForumDetailResultModel>>>
    {
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IMapper _mapper;

        public GetClassForumDetailResultsQueryHandler(IClassForumDetailResultRepository classForumDetailResultRepository,
                                                      IMapper mapper)
        {
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ClassForumDetailResultModel>>> Handle(GetClassForumDetailResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumDetailResultModel>> methodResult = new MethodResult<IList<ClassForumDetailResultModel>>();

            var classForumDetailResults = await _classForumDetailResultRepository.Queryable
                                                                                 .Include(x => x.ClassForumResult)
                                                                                 .ThenInclude(x => x.ClassForum)
                                                                                 .Include(x => x.ClassForumResultFiles)
                                                                                 .Include(x => x.ClassForumDetailResultHistories)
                                                                                 .ThenInclude(x => x.ClassForumResultFiles)
                                                                                 .Where(x => x.ClassForumResult!.LessonResultId == request.LessonResultId)
                                                                                 .ToListAsync(cancellationToken);

            if (classForumDetailResults == null || !classForumDetailResults.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumDetailResults));
                return methodResult;
            }

            foreach (var item in classForumDetailResults)
            {
                item.Score = GetTargetCount(item);
            }

            methodResult.Result = _mapper.Map<IList<ClassForumDetailResultModel>>(classForumDetailResults);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static int GetTargetCount(ClassForumDetailResult classForumDetailResult)
        {
            int targetScore = default;
            var classForum = classForumDetailResult.ClassForumResult?.ClassForum;
            if (classForum?.CourseSkill == EnumCourseSkill.Writing && classForum?.TaggetWordLimit <= classForumDetailResult.WordCount)
            {
                ++targetScore;
            }
            if (classForum?.CourseSkill == EnumCourseSkill.Speaking && classForum?.TaggetTimeLimit <= classForumDetailResult.TimeCount)
            {
                ++targetScore;
            }
            return targetScore;
        }
    }
}
