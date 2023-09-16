// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
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
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public SearchReviewTeacherRatingDetailQueryHandler(IUserService userService, IVideoResultRepository videoResultRepository, IVideoRepository videoRepository, ILessonResultRepository lessonResultRepository, ICourseRepository courseRepository, IClassForumRepository classForumRepository, IClassForumResultRepository classForumResultRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, IStudentFeedbackRepository studentFeedbackRepository)
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
                                       ReviewArea = nameof(Video),
                                       Starts = baseQ.NumberOfStars
                                   };

            var classFormQuery = from baseQ in _classForumResultRepository.Queryable
                                 join cl in _classForumRepository.Queryable on baseQ.ClassForumId equals cl.Id
                                 join lr in _lessonResultRepository.Queryable on baseQ.LessonResultId equals lr.Id
                                 join c in _courseRepository.Queryable on lr.CourseId equals c.Id
                                 join s in _studentFeedbackRepository.Queryable on cl.Id equals s.ObjectId
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
                                     Starts = s.FeedBackStars ?? default
                                 };

            var mockTestQuery = from baseQ in _mockTestResultRepository.Queryable
                                join m in _mockTestRepository.Queryable on baseQ.MockTestId equals m.Id
                                join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                where baseQ.GradingTeacherId == request.TeacherId && baseQ.Status == EnumResultStatus.Done
                                select new ReviewTeacherRatingDetailModel
                                {
                                    Id = baseQ.Id,
                                    Code = c.Code,
                                    CreatedDate = baseQ.CreatedDate,
                                    CreatedFullName = baseQ.CreatedFullName,
                                    CreatedUserId = baseQ.CreatedUserId,
                                    Feedback = baseQ.FeedBackNote,
                                    ReviewArea = nameof(MockTest),
                                    Starts = baseQ.FeedBackStars ?? default
                                };

            var query = mockTestQuery.AsEnumerable().Union(classFormQuery.AsEnumerable()).Union(videoResultQuery.AsEnumerable());
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName != null && m.CreatedFullName.Contains(request.Keyword, StringComparison.CurrentCulture)));
            }
            int totalItem = query.Count();
            var lists = query.OrderBy(x => x.CreatedDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            methodResult.Result = new ReviewTeacherRatingDetailSearchModel { FullName = teacher?.Human?.FullName, PagingItemsModel = new PagingItemsModel<ReviewTeacherRatingDetailModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
