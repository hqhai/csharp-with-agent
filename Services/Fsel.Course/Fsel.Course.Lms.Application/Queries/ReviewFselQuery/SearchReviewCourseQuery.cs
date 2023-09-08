// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
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
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
            var starts = studentReviews.Where(x => x.StudentReviewDetails != null).SelectMany(x => x.StudentReviewDetails!).Average(x => x.VoteStars);
            var courseIds = studentReviews.Select(x => x.CourseId ?? Guid.Empty).Distinct().ToList();

            var courseQuery = _courseRepository.Queryable.Where(x => courseIds.Contains(x.Id))
                .Select(x => new ReviewCourseModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    UpdatedDate = x.UpdatedDate,
                    UpdatedFullName = x.UpdatedFullName,
                    UpdatedUserId = x.UpdatedUserId,
                    Code = x.Code,
                    CourseLevel = x.CourseLevel,
                });

            if (request.CourseLevel != null)
            {
                courseQuery = courseQuery.Where(x => x.CourseLevel == request.CourseLevel);
                courseIds = await courseQuery.Select(x => x.Id).Distinct().ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                starts = (courseIds != null && courseIds.Count > 0) ? Math.Round(studentReviews.Where(x => x.StudentReviewDetails != null && courseIds.Contains(x.CourseId ?? default)).SelectMany(x => x.StudentReviewDetails!).Average(x => x.VoteStars), 1) : default;
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            foreach (var item in lists)
            {
                var studentReview = studentReviews.FirstOrDefault(a => a.CourseId == item.Id);
                item.Starts = studentReview?.StudentReviewDetails?.Average(x => x.VoteStars) ?? default;
            }

            methodResult.Result = new ReviewCourseSearchModel { Starts = starts, PagingItemsModel = new PagingItemsModel<ReviewCourseModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
