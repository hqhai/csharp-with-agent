// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.FselReviews;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchPlatformReviewQuery : SearchReviewFselQueryModel, IRequest<MethodResult<StudentReviewSearchModel>>
    {
    }

    public class SearchPlatformReviewQueryHandler : IRequestHandler<SearchPlatformReviewQuery, MethodResult<StudentReviewSearchModel>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;

        public SearchPlatformReviewQueryHandler(IStudentReviewRepository studentReviewRepository)
        {
            _studentReviewRepository = studentReviewRepository;
        }

        public async Task<MethodResult<StudentReviewSearchModel>> Handle(SearchPlatformReviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentReviewSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
                .Where(x => x.ReviewType == EnumReviewType.Platform).Select(x => new StudentReviewTypeModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    ReviewType = x.ReviewType,
                    StudentId = x.StudentId,
                    Starts = Math.Round(x.StudentReviewDetails.Average(x => x.VoteStars), 1),
                    StudentReviewQuestionTypes = x.StudentReviewDetails.Select(x => new StudentReviewQuestionTypeModel
                    {
                        Id = x.Id,
                        Content = x.Content,
                        ReviewQuestionType = x.ReviewQuestionType,
                        VoteStars = x.VoteStars,
                    }).ToList()
                });
            var starts = NumberHelper.ConvertDoubleDecimal(await query.AverageAsync(x => x.Starts, cancellationToken).ConfigureAwait(false));
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new StudentReviewSearchModel { Starts = starts, PagingItems = new PagingItemsModel<StudentReviewTypeModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
