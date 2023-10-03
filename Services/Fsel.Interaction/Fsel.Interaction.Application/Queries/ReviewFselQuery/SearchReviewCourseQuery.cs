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
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Domain.Entities;
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
            var query = await _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
            .Where(x => x.ReviewType == EnumReviewType.Course && x.CourseId.HasValue && courseIds.Contains(x.CourseId.Value))
            .Select(x => GetCourse(x, courses)).ToListAsync(cancellationToken);
            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5).ToList();
            }

            if (request.CourseLevel != null)
            {
                query = query.Where(x => x.CourseLevel == request.CourseLevel).ToList();
            }
            var stars = query.Any() ? NumberHelper.ConvertDoubleDecimal(query.Average(x => x.Stars)) : default;
            int totalItem = query.Count;
            var lists = query.ApplySortAndPaging(request).ToList();
            foreach (var item in lists)
            {
                item.Stars = NumberHelper.ConvertDoubleDecimal(item.Stars);
            }
            methodResult.Result = new CourseReviewSearchModel { Stars = stars, PagingItemsModel = new PagingItemsModel<CourseReviewModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static CourseReviewModel GetCourse(StudentReview x, IList<CourseModel>? courses)
        {
            var course = courses?.FirstOrDefault(y => y.Id == x.CourseId);
            return new CourseReviewModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                Code = course?.Code,
                CourseLevel = course?.CourseLevel ?? default,
                UpdatedDate = x.UpdatedDate,
                UpdatedFullName = x.UpdatedFullName,
                UpdatedUserId = x.UpdatedUserId,
                CourseId = x.CourseId,
                Stars = x.StudentReviewDetails.Average(x => x.VoteStars)
            };
        }
    }
}
