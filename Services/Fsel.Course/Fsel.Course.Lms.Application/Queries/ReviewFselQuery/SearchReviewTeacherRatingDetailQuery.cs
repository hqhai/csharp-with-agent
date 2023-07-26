// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewTeacherRatingDetailQuery : SearchReviewTeacherRatingDetailQueryModel, IRequest<MethodResult<ReviewTeacherRatingDetailSearchModel>>
    {
    }

    public class SearchReviewTeacherRatingDetailQueryHandler : IRequestHandler<SearchReviewTeacherRatingDetailQuery, MethodResult<ReviewTeacherRatingDetailSearchModel>>
    {
        private readonly IUserService _userService;
        private readonly IVideoResultRepository _videoResultRepository;

        public SearchReviewTeacherRatingDetailQueryHandler(IUserService userService, IVideoResultRepository videoResultRepository)
        {
            _userService = userService;
            _videoResultRepository = videoResultRepository;
        }

        public async Task<MethodResult<ReviewTeacherRatingDetailSearchModel>> Handle(SearchReviewTeacherRatingDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewTeacherRatingDetailSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var teacherResults = await _userService.GetTeacherByIdAsync(request.TeacherId);
            if (!teacherResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teacher = teacherResults.Content?.Result;

            var videoResultQuery = _videoResultRepository.Queryable.Include(x => x.Video)
                .Include(x => x.LessonResult)
                .ThenInclude(x => x!.Course)
                .Where(x => x.Video!.TeacherId == request.TeacherId)
                .Select(x => new ReviewTeacherRatingDetailModel
                {
                    Id = x.Id,
                    Code = x.LessonResult!.Course!.Code,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    Feedback = x.Feedback,
                    ReviewArea = "Video Lesson",
                    Scores = x.NumberOfStars
                });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                videoResultQuery = videoResultQuery.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName != null && m.CreatedFullName.Contains(request.Keyword, StringComparison.CurrentCulture)));
            }

            int totalItem = await videoResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await videoResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new ReviewTeacherRatingDetailSearchModel { FullName = teacher?.Human?.FullName, PagingItemsModel = new PagingItemsModel<ReviewTeacherRatingDetailModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
