// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.FselReviews;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewCourseQuery : SearchReviewFselQueryModel, IRequest<MethodResult<CourseReviewSearchModel>>
    {
    }

    public class SearchReviewCourseQueryHandler : IRequestHandler<SearchReviewCourseQuery, MethodResult<CourseReviewSearchModel>>
    {
        private readonly ICourseService _courseService;
        private readonly IStudentReviewRepository _studentReviewRepository;

        public SearchReviewCourseQueryHandler(ICourseService courseService, IStudentReviewRepository studentReviewRepository)
        {
            _courseService = courseService;
            _studentReviewRepository = studentReviewRepository;
        }

        public async Task<MethodResult<CourseReviewSearchModel>> Handle(SearchReviewCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseReviewSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var courseIds = await _studentReviewRepository.Queryable.Where(x => x.ReviewType == EnumReviewType.Course).Select(x => x.CourseId ?? default).ToListAsync(cancellationToken);
            var courseResults = await _courseService.GetListCourseByIds(courseIds);
            if (!courseResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(courseResults));
                return methodResult;
            }
            var courses = courseResults.Content?.Result;
            if (request.CourseLevel.HasValue)
            {
                courses = courses?.Where(x => x.CourseLevel == request.CourseLevel).ToList();
            }
            courseIds = courses?.Select(x => x.Id).ToList();
            if (courseIds == null || !courseIds.Any())
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var query = _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
            .Where(x => x.ReviewType == EnumReviewType.Course && x.CourseId.HasValue && courseIds.Contains(x.CourseId.Value))
            .Select(x => new CourseReviewModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                UpdatedDate = x.UpdatedDate,
                UpdatedFullName = x.UpdatedFullName,
                UpdatedUserId = x.UpdatedUserId,
                CourseId = x.CourseId,
                Stars = x.StudentReviewDetails.Average(x => x.VoteStars)
            });
            var starts = NumberHelper.ConvertDoubleDecimal(await query.AverageAsync(x => x.Stars, cancellationToken));
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            foreach (var item in lists)
            {
                var course = courses?.FirstOrDefault(x => x.Id == item.CourseId);
                if (course != null)
                {
                    item.CourseLevel = course.CourseLevel;
                }
            }
            methodResult.Result = new CourseReviewSearchModel { Starts = starts, PagingItemsModel = new PagingItemsModel<CourseReviewModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
