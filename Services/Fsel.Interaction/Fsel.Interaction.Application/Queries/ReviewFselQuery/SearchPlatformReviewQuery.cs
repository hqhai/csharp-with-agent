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
                    Stars = x.StudentReviewDetails.Average(x => x.VoteStars),
                    StudentReviewQuestionTypes = x.StudentReviewDetails.Select(x => new StudentReviewQuestionTypeModel
                    {
                        Id = x.Id,
                        Content = x.Content,
                        ReviewQuestionType = x.ReviewQuestionType,
                        VoteStars = x.VoteStars,
                    }).ToList()
                });
            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5);
            }
            var result = await query.ToListAsync(cancellationToken);
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var stars = totalItem > 0 ? NumberHelper.ConvertRatingToDouble(result.Average(x => x.Stars)) : default;
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            foreach (var item in lists)
            {
                item.Stars = NumberHelper.ConvertRatingToDouble(item.Stars);
            }
            methodResult.Result = new StudentReviewSearchModel { Stars = stars, PagingItems = new PagingItemsModel<StudentReviewTypeModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
