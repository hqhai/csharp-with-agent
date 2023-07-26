// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReviewCourseQuery : SearchReviewCourseQueryModel, IRequest<MethodResult<ReviewCourseSearchModel>>
    {
    }

    public class SearchReviewCourseQueryHandler : IRequestHandler<SearchReviewCourseQuery, MethodResult<ReviewCourseSearchModel>>
    {
        private readonly IInteractionService _interactionService;
        private readonly ICourseRepository _courseRepository;

        public SearchReviewCourseQueryHandler(IInteractionService interactionService, ICourseRepository courseRepository)
        {
            _interactionService = interactionService;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<ReviewCourseSearchModel>> Handle(SearchReviewCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewCourseSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var studentReviewResults = await _interactionService.GetStudentReviewsAsync(EnumReviewType.Course);
            if (!studentReviewResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallInteractionServiceError));
                return methodResult;
            }
            var studentReviews = studentReviewResults?.Content?.Result?.ToList() ?? new List<StudentReviewModel>();
            var scores = studentReviews.Where(x => x.StudentReviewDetails != null).SelectMany(x => x.StudentReviewDetails!).Average(x => x.VoteStars);

            var courseIds = studentReviews.Select(x => x.CourseId ?? Guid.Empty).Distinct().ToList();
            var courses = await _courseRepository.GetByIdsAsync(courseIds);
            foreach (var item in studentReviews)
            {
                var course = courses.FirstOrDefault(x => x.Id == item.CourseId);
                if (course != null)
                {
                    item.Code = course.Code;
                    item.CourseLevel = course.CourseLevel;
                }
            }
            var studentReviewQuery = studentReviews.Select(x => new ReviewCourseModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                CourseId = x.CourseId,
                ReviewType = x.ReviewType,
                StudentId = x.StudentId,
                Code = x.Code,
                CourseLevel = x.CourseLevel,
                Scores = x.StudentReviewDetails != null ? x.StudentReviewDetails.Average(x => x.VoteStars) : 0,
                StudentReviewDetails = x.StudentReviewDetails?.Select(x => new ReviewCourseDetailModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    ReviewQuestionType = x.ReviewQuestionType,
                    VoteStars = x.VoteStars,
                }).ToList(),
            }).AsEnumerable();

            if (request.CourseLevel != null)
            {
                studentReviewQuery = studentReviewQuery.Where(x => x.CourseLevel == request.CourseLevel);
            }

            int totalItem = studentReviewQuery.Count();
            var lists = studentReviewQuery.Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();

            methodResult.Result = new ReviewCourseSearchModel { Scores = scores, PagingItemsModel = new PagingItemsModel<ReviewCourseModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
