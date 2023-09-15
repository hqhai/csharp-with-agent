// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReviewAIQuery : SearchReviewAIQueryModel, IRequest<MethodResult<ReviewCourseByClassForumAIModel>>
    {
    }

    public class SearchReviewAIQueryHandler : IRequestHandler<SearchReviewAIQuery, MethodResult<ReviewCourseByClassForumAIModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;

        public SearchReviewAIQueryHandler(ICourseRepository courseRepository, ILessonResultRepository lessonResultRepository, ILessonRepository lessonRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository, IUnitRepository unitRepository, IUnitLessonRepository unitLessonRepository)
        {
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitRepository = unitRepository;
            _unitLessonRepository = unitLessonRepository;
        }

        public async Task<MethodResult<ReviewCourseByClassForumAIModel>> Handle(SearchReviewAIQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewCourseByClassForumAIModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = from baseQ in _lessonResultRepository.Queryable
                        join l in _lessonRepository.Queryable on baseQ.LessonId equals l.Id
                        select baseQ;

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
