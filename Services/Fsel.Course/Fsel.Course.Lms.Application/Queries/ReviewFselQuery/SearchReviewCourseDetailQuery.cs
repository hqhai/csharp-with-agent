// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReviewCourseDetailQuery : SearchReviewCourseDetailQueryModel, IRequest<MethodResult<ReviewCourseDetailSearchModel>>
    {
    }

    public class SearchReviewCourseDetailQueryHandler : IRequestHandler<SearchReviewCourseDetailQuery, MethodResult<ReviewCourseDetailSearchModel>>
    {
        private readonly IInteractionService _interactionService;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public SearchReviewCourseDetailQueryHandler(IInteractionService interactionService, IUserService userService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _interactionService = interactionService;
            _userService = userService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<ReviewCourseDetailSearchModel>> Handle(SearchReviewCourseDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewCourseDetailSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var studentReviewResults = await _interactionService.GetStudentReviewsAsync(EnumReviewType.Course);
            if (!studentReviewResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallInteractionServiceError));
                return methodResult;
            }
            var studentReviews = studentReviewResults?.Content?.Result?.Where(x => x.CourseId == request.CourseId).ToList() ?? new List<StudentReviewModel>();
            if (!studentReviews.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var starts = studentReviews.Where(x => x.StudentReviewDetails != null).SelectMany(x => x.StudentReviewDetails!).Average(x => x.VoteStars);
            var studentReviewQuery = studentReviews.Select(x => new ReviewCourseDetailModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                CourseId = x.CourseId,
                CourseLevel = course.CourseLevel,
                ReviewType = x.ReviewType,
                StudentId = x.StudentId,
                Starts = x.StudentReviewDetails != null ? x.StudentReviewDetails.Average(x => x.VoteStars) : default,
                StudentReviewDetails = x.StudentReviewDetails?.Select(x => new ReviewCourseDetailInfoModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    ReviewQuestionType = x.ReviewQuestionType,
                    VoteStars = x.VoteStars,
                }).ToList(),
            }).AsEnumerable();

            int totalItem = studentReviewQuery.Count();

            var lists = studentReviewQuery.OrderBy(x => x.CreatedDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            lists = request.IsSortDesc ? lists.OrderByDescending(m => m.CreatedFullName).ToList() : lists.OrderBy(m => m.CreatedFullName).ToList();
            if (request.IsSortStarts.HasValue)
            {
                lists = request.IsSortStarts.Value ? lists.OrderByDescending(m => m.Starts).ToList() : lists.OrderBy(m => m.Starts).ToList();
            }
            var studentIds = lists.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result;

            var classStudentResults = await _trainingService.GetClassByStudentIdsAsync(new GetClassListByStudentIdsModel { CourseId = request.CourseId, StudentIds = studentIds });
            var classeStudents = classStudentResults.Content?.Result;
            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                var classStudent = classeStudents?.FirstOrDefault(x => x.StudentId == item.StudentId);
                item.Code = course.Code;
                item.CodeStudent = student?.Human?.Code;
                item.ClassCode = classStudent?.Code;
            }

            methodResult.Result = new ReviewCourseDetailSearchModel { Starts = starts, Code = course.Code, PagingItemsModel = new PagingItemsModel<ReviewCourseDetailModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
