// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
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
        private readonly IVideoRepository _videoRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseRepository _courseRepository;

        public SearchReviewTeacherRatingDetailQueryHandler(IUserService userService
            , IVideoResultRepository videoResultRepository
            , IVideoRepository videoRepository
            , ILessonResultRepository lessonResultRepository
            , ICourseRepository courseRepository
            )
        {
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
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

            var videoResultQuery = from baseQ in _videoResultRepository.Queryable
                                   join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                   join lr in _lessonResultRepository.Queryable on baseQ.LessonResultId equals lr.Id
                                   join c in _courseRepository.Queryable on lr.CourseId equals c.Id
                                   where v.TeacherId == request.TeacherId && baseQ.Status == EnumResultStatus.Done
                                   select new ReviewTeacherRatingDetailModel
                                   {
                                       Id = baseQ.Id,
                                       Code = c.Code,
                                       CreatedDate = baseQ.CreatedDate,
                                       CreatedFullName = baseQ.CreatedFullName,
                                       CreatedUserId = baseQ.CreatedUserId,
                                       Feedback = baseQ.Feedback,
                                       ReviewArea = "Video Lesson",
                                       Starts = baseQ.NumberOfStars
                                   };

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
