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

    public class SearchReviewCommunicationQuery : SearchReviewCommunicationQueryModel, IRequest<MethodResult<ReviewCommunicationSearchModel>>
    {
    }

    public class SearchReviewCommunicationQueryHandler : IRequestHandler<SearchReviewCommunicationQuery, MethodResult<ReviewCommunicationSearchModel>>
    {
        private readonly IInteractionService _interactionService;

        public SearchReviewCommunicationQueryHandler(IInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public async Task<MethodResult<ReviewCommunicationSearchModel>> Handle(SearchReviewCommunicationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewCommunicationSearchModel>();

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
            var studentReviewQuery = studentReviews.Select(x => new ReviewCommunicationModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                ReviewType = x.ReviewType,
                StudentId = x.StudentId,
                Starts = x.StudentReviewDetails != null ? x.StudentReviewDetails.Average(x => x.VoteStars) : 0,
                StudentReviewDetails = x.StudentReviewDetails?.Select(x => new ReviewCommunicationStudentModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    ReviewQuestionType = x.ReviewQuestionType,
                    VoteStars = x.VoteStars,
                }).ToList(),
            }).AsEnumerable();

            int totalItem = studentReviewQuery.Count();
            var lists = studentReviewQuery.Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();

            methodResult.Result = new ReviewCommunicationSearchModel { Starts = starts, PagingItemsModel = new PagingItemsModel<ReviewCommunicationModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
