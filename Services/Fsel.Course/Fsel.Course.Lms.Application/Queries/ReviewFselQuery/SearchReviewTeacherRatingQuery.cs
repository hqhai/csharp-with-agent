// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewTeacherRatingQuery : SearchReviewTeacherRatingQueryModel, IRequest<MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>>
    {
    }

    public class SearchReviewTeacherRatingQueryHandler : IRequestHandler<SearchReviewTeacherRatingQuery, MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>>
    {
        private readonly IUserService _userService;
        private readonly IVideoRepository _videoRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public SearchReviewTeacherRatingQueryHandler(IUserService userService
            , IVideoRepository videoRepository
            , IClassForumRepository classForumRepository
            , IMockTestRepository mockTestRepository)
        {
            _userService = userService;
            _videoRepository = videoRepository;
            _classForumRepository = classForumRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>> Handle(SearchReviewTeacherRatingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var videos = await _videoRepository.Queryable.Include(x => x.VideoResults).Where(x => x.VideoResults.Count > 0).ToListAsync(cancellationToken);
            var mockTests = await _mockTestRepository.Queryable.Include(x => x.MockTestResults).ToListAsync(cancellationToken);
            var classForums = await _classForumRepository.Queryable.Include(x => x.ClassForumResults).ToListAsync(cancellationToken);

            var teacherVideoIds = videos.Select(x => x.TeacherId).Distinct().AsEnumerable();
            var teacherMockTestIds = mockTests.SelectMany(x => x.MockTestResults).Where(x => x.GradingTeacherId != null).Select(x => x.GradingTeacherId ?? default).Distinct().AsEnumerable();
            var teacherClassForumIds = classForums.SelectMany(x => x.ClassForumResults).Where(x => x.GradingTeacherId != null).Select(x => x.GradingTeacherId ?? default).Distinct().AsEnumerable();

            var teacherIds = teacherVideoIds.Union(teacherMockTestIds).Union(teacherClassForumIds).Distinct().ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            if (!teacherResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teachers = teacherResults.Content?.Result;

            var teacherRatingQuery = teachers?.OrderBy(x => x.CreatedDate).Select(x => new ReviewTeacherRatingSearchModel
            {
                Id = x.Id,
                Code = x.Human!.Code,
                CreatedDate = x.CreatedDate,
                FullName = x.Human.FullName,
            }).AsEnumerable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                teacherRatingQuery = teacherRatingQuery?.Where(m => m.Id.ToString() == request.Keyword || (m.FullName != null && m.FullName.Contains(request.Keyword, StringComparison.CurrentCulture)));
            }

            int totalItem = teacherRatingQuery!.Count();
            var lists = teacherRatingQuery!.OrderBy(x => x.CreatedDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            foreach (var item in lists)
            {
                var listVideo = videos.Where(x => x.TeacherId == item.Id).ToList();
                var videoResults = listVideo.Where(x => x.VideoResults.Count > 0).SelectMany(x => x.VideoResults).Where(x => x.Status == EnumResultStatus.Done).ToList();
                var listMockTest = mockTests.SelectMany(x => x.MockTestResults).Where(x => x.GradingTeacherId == item.Id && x.FeedBackStars.HasValue).ToList();
                var listClassForum = classForums.SelectMany(x => x.ClassForumResults).Where(x => x.GradingTeacherId == item.Id && x.FeedBackStars.HasValue).ToList();

                var starts = new List<double>();
                starts.Add(videoResults.Any() ? Math.Round(videoResults.Average(x => x.NumberOfStars), 1) : default);
                starts.Add(listMockTest.Any() ? Math.Round(listMockTest.Average(x => x.FeedBackStars ?? default), 1) : default);
                starts.Add(listClassForum.Any() ? Math.Round(listClassForum.Average(x => x.FeedBackStars ?? default), 1) : default);
                item.Starts = starts.Any() ? Math.Round(starts.Average(), 1) : default;
            }

            methodResult.Result = new PagingItemsModel<ReviewTeacherRatingSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
