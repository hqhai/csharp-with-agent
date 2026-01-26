// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

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
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public SearchReviewTeacherRatingDetailQueryHandler(IUserService userService, IVideoResultRepository videoResultRepository, IVideoRepository videoRepository, ILessonResultRepository lessonResultRepository, ICourseRepository courseRepository, IClassForumRepository classForumRepository, IClassForumResultRepository classForumResultRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, IStudentFeedbackRepository studentFeedbackRepository, ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _studentFeedbackRepository = studentFeedbackRepository;
            _courseResultRepository = courseResultRepository;
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

            var result = new ReviewTeacherRatingDetailSearchModel
            {
                FullName = teacher?.User?.FullName
            };

            var videoResultQuery = from baseQ in _videoResultRepository.Queryable
                                   join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                   join lr in _lessonResultRepository.Queryable on baseQ.LessonResultId equals lr.Id
                                   join cr in _courseResultRepository.Queryable on lr.CourseResultId equals cr.Id
                                   join c in _courseRepository.Queryable on cr.CourseId equals c.Id
                                   where v.TeacherId == request.TeacherId && baseQ.Status == EnumResultStatus.Done
                                   select new ReviewTeacherRatingDetailModel
                                   {
                                       Id = baseQ.Id,
                                       Code = c.Code,
                                       CreatedDate = baseQ.CreatedDate,
                                       CreatedFullName = baseQ.CreatedFullName,
                                       CreatedUserId = baseQ.CreatedUserId,
                                       Feedback = baseQ.Feedback,
                                       ReviewArea = nameof(Video),
                                       Stars = baseQ.NumberOfStars
                                   };

            var classFormQuery = from baseQ in _classForumResultRepository.Queryable
                                 join cl in _classForumRepository.Queryable on baseQ.ClassForumId equals cl.Id
                                 join lr in _lessonResultRepository.Queryable on baseQ.LessonResultId equals lr.Id
                                 join cr in _courseResultRepository.Queryable on lr.CourseResultId equals cr.Id
                                 join c in _courseRepository.Queryable on cr.CourseId equals c.Id
                                 join s in _studentFeedbackRepository.Queryable on baseQ.Id equals s.ObjectId
                                 where baseQ.GradingTeacherId == request.TeacherId && baseQ.Status == EnumClassForumResultStatus.Graded
                                 select new ReviewTeacherRatingDetailModel
                                 {
                                     Id = baseQ.Id,
                                     Code = c.Code,
                                     CreatedDate = baseQ.CreatedDate,
                                     CreatedFullName = baseQ.CreatedFullName,
                                     CreatedUserId = baseQ.CreatedUserId,
                                     ReviewArea = nameof(ClassForum),
                                     Feedback = s.FeedBackNote,
                                     Stars = s.FeedBackStars ?? default,
                                     FeedbackNegative = s.FeedBackNegatives,
                                     FeedbackPositive = s.FeedBackPositives,
                                 };

            var mockTestQuery = from baseQ in _mockTestResultRepository.Queryable
                                join m in _mockTestRepository.Queryable on baseQ.MockTestId equals m.Id
                                join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                join s in _studentFeedbackRepository.Queryable on baseQ.Id equals s.ObjectId
                                where baseQ.GradingTeacherId == request.TeacherId && baseQ.Status == EnumResultStatus.Done
                                select new ReviewTeacherRatingDetailModel
                                {
                                    Id = baseQ.Id,
                                    Code = c.Code,
                                    CreatedDate = baseQ.CreatedDate,
                                    CreatedFullName = baseQ.CreatedFullName,
                                    CreatedUserId = baseQ.CreatedUserId,
                                    ReviewArea = nameof(MockTest),
                                    Feedback = s.FeedBackNote,
                                    Stars = s.FeedBackStars ?? default,
                                    FeedbackNegative = s.FeedBackNegatives,
                                    FeedbackPositive = s.FeedBackPositives,
                                };

            var query = mockTestQuery.AsEnumerable().Union(classFormQuery.AsEnumerable()).Union(videoResultQuery.AsEnumerable());
            var userIds = query.Select(x => x.CreatedUserId).Distinct().ToList();
            var userResults = await _userService.GetUsersByUserIdsAsync(userIds);
            var users = userResults?.Content?.Result;
            foreach (var item in query)
            {
                item.CreatedFullName = users?.FirstOrDefault(x => x.Id == item.CreatedUserId)?.FullName ?? item.CreatedFullName;
            }

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    query = query.Where(m => m.CreatedFullName != null && m.CreatedFullName.Contains(request.Keyword, StringComparison.InvariantCulture));
                }
            }
            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5);
            }
            int totalItem = query.Count();
            var lists = query.ApplySortAndPaging(request).ToList();

            foreach (var item in lists)
            {
                item.Stars = NumberHelper.ConvertRound(item.Stars);
            }
            result.PagingItemsModel = new PagingItemsModel<ReviewTeacherRatingDetailModel>(lists, request, totalItem);
            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
