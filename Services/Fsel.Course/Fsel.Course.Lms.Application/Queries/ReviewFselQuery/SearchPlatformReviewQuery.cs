// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchPlatformReviewQuery : SearchPlatformReviewQueryModel, IRequest<MethodResult<ReviewPlatformSearchModel>>
    {
    }

    public class SearchPlatformReviewQueryHandler : IRequestHandler<SearchPlatformReviewQuery, MethodResult<ReviewPlatformSearchModel>>
    {
        private readonly IInteractionService _interactionService;

        public SearchPlatformReviewQueryHandler(IInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public async Task<MethodResult<ReviewPlatformSearchModel>> Handle(SearchPlatformReviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewPlatformSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var studentReviewResults = await _interactionService.GetStudentReviewsAsync(EnumReviewType.Platform);
            if (!studentReviewResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                return methodResult;
            }
            var studentReviews = studentReviewResults?.Content?.Result?.ToList() ?? new List<StudentReviewModel>();
            var starts = studentReviews.Where(x => x.StudentReviewDetails != null).SelectMany(x => x.StudentReviewDetails!).Average(x => x.VoteStars);
            var studentReviewQuery = studentReviews.Select(x => new ReviewPlatformModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                ReviewType = x.ReviewType,
                StudentId = x.StudentId,
                Starts = x.StudentReviewDetails != null ? Math.Round(x.StudentReviewDetails.Average(x => x.VoteStars), 1) : default,
                StudentReviewDetails = x.StudentReviewDetails?.Select(x => new ReviewPlatformStudentModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    ReviewQuestionType = x.ReviewQuestionType,
                    VoteStars = x.VoteStars,
                }).ToList(),
            }).AsEnumerable();

            int totalItem = studentReviewQuery.Count();
            var sortName = request.IsSortDesc ? studentReviewQuery.OrderByDescending(m => m.CreatedFullName) : studentReviewQuery.OrderBy(m => m.CreatedFullName);
            var sortStarts = request.IsSortStarts ? sortName.OrderByDescending(m => m.Starts) : sortName.OrderBy(m => m.Starts);
            var lists = sortStarts.OrderBy(x => x.CreatedDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            methodResult.Result = new ReviewPlatformSearchModel { Starts = starts, PagingItems = new PagingItemsModel<ReviewPlatformModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
