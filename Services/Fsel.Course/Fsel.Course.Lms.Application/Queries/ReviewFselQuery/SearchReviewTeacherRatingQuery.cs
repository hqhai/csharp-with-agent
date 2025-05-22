// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System.Globalization;
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
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
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public SearchReviewTeacherRatingQueryHandler(IUserService userService, IVideoRepository videoRepository, IClassForumResultRepository classForumResultRepository, IMockTestResultRepository mockTestResultRepository, IStudentFeedbackRepository studentFeedbackRepository)
        {
            _userService = userService;
            _videoRepository = videoRepository;
            _classForumResultRepository = classForumResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _studentFeedbackRepository = studentFeedbackRepository;
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
            var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.GradingTeacherId != null).ToListAsync(cancellationToken);
            var classForumResults = await _classForumResultRepository.Queryable.Where(x => x.GradingTeacherId != null).ToListAsync(cancellationToken);

            var classForumResultIds = classForumResults.Select(x => x.Id).ToList();
            var mockTestResultIds = mockTestResults.Select(x => x.Id).ToList();

            var feedbackClassForumResults = await _studentFeedbackRepository.Queryable.Where(x => classForumResultIds.Contains(x.ObjectId)).ToListAsync(cancellationToken);
            var feedbackMockTestResults = await _studentFeedbackRepository.Queryable.Where(x => mockTestResultIds.Contains(x.ObjectId)).ToListAsync(cancellationToken);

            var teacherVideoIds = videos.Select(x => x.TeacherId).Distinct().ToList();
            var teacherMockTestIds = mockTestResults.Select(x => x.GradingTeacherId ?? default).Distinct().ToList();
            var teacherClassForumIds = classForumResults.Select(x => x.GradingTeacherId ?? default).Distinct().ToList();

            var teacherIds = teacherVideoIds.Union(teacherMockTestIds).Union(teacherClassForumIds).Distinct().ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            if (!teacherResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teachers = teacherResults.Content?.Result;
            var query = teachers?.OrderBy(x => x.CreatedDate).Select(x => new ReviewTeacherRatingSearchModel
            {
                Id = x.Id,
                Code = x.User?.Code,
                CreatedDate = x.CreatedDate,
                FullName = x.User?.FullName,
            }).ToList();

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query?.Where(m => m.Id == guid).ToList();
                }
                else
                {
                    query = query?.Where(m => m.FullName != null && m.FullName!.Contains(request.Keyword, StringComparison.InvariantCulture)).ToList();
                }
            }
            if (query != null && query.Any())
            {
                foreach (var item in query)
                {
                    var listVideo = videos.Where(x => x.TeacherId == item.Id).ToList();
                    var videoResults = listVideo.Where(x => x.VideoResults.Count > 0).SelectMany(x => x.VideoResults).Where(x => x.Status == EnumResultStatus.Done).ToList();
                    mockTestResultIds = mockTestResults.Where(x => x.GradingTeacherId == item.Id).Select(x => x.Id).ToList();
                    classForumResultIds = classForumResults.Where(x => x.GradingTeacherId == item.Id).Select(x => x.Id).ToList();
                    var mockTestFeedbacks = feedbackMockTestResults.Where(x => mockTestResultIds.Contains(x.ObjectId));
                    var classForumFeedbacks = feedbackClassForumResults.Where(x => classForumResultIds.Contains(x.ObjectId));

                    var stars = new List<double>();
                    if (videoResults.Any())
                    {
                        stars.Add(videoResults.Average(x => x.NumberOfStars));
                    }
                    if (classForumFeedbacks.Any())
                    {
                        stars.Add(classForumFeedbacks.Average(x => x.FeedBackStars ?? default));
                    }
                    if (mockTestFeedbacks.Any())
                    {
                        stars.Add(mockTestFeedbacks.Average(x => x.FeedBackStars ?? default));
                    }
                    item.Stars = stars.Any() ? NumberHelper.ConvertRound(stars.Average()) : default;
                }
                if (request.NumberOfStars != null)
                {
                    query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5).ToList();
                }
            }

            int totalItem = query?.Count ?? default;
            var lists = query?.ApplySortAndPaging(request).ToList();
            methodResult.Result = new PagingItemsModel<ReviewTeacherRatingSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
