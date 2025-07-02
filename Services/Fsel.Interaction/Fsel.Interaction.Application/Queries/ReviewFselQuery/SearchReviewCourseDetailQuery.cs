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
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.TrainingServices.Models;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
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

    public class SearchReviewCourseDetailQuery : SearchReviewFselQueryModel, IRequest<MethodResult<StudentReviewSearchModel>>
    {
    }

    public class SearchReviewCourseDetailQueryHandler : IRequestHandler<SearchReviewCourseDetailQuery, MethodResult<StudentReviewSearchModel>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly ICourseService _courseService;

        public SearchReviewCourseDetailQueryHandler(IStudentReviewRepository studentReviewRepository, IUserService userService, ITrainingService trainingService, ICourseService courseService)
        {
            _studentReviewRepository = studentReviewRepository;
            _userService = userService;
            _trainingService = trainingService;
            _courseService = courseService;
        }

        public async Task<MethodResult<StudentReviewSearchModel>> Handle(SearchReviewCourseDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentReviewSearchModel>();

            if (request.PageSize > 100 || !request.CourseId.HasValue)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var courseResults = await _courseService.GetListCourseByIds(new List<Guid> { request.CourseId.Value });
            if (!courseResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(courseResults));
                return methodResult;
            }
            var course = courseResults?.Content?.Result?.FirstOrDefault();
            var studentReviews = await _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
                                                                            .Where(x => x.ReviewType == EnumReviewType.Course && x.CourseId == request.CourseId)
                                                                            .ToListAsync(cancellationToken);

            var studentIds = studentReviews.Select(x => x.StudentId).ToList();
            var classStudentResults = await _trainingService.GetClassByStudentIdsAsync(new GetClassListByStudentIdsModel { CourseId = request.CourseId, StudentIds = studentIds });
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var classeStudents = classStudentResults.Content?.Result;
            var students = studentResults.Content?.Result;
            var query = studentReviews.Select(x => GetStudentReview(students, x, classeStudents)).ToList();
            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5).ToList();
            }
            var stars = query.Any() ? NumberHelper.ConvertRound(query.Average(x => x.Stars)) : default;
            int totalItem = query.Count;
            var lists = query.ApplySortAndPaging(request).ToList();

            foreach (var item in lists)
            {
                item.Stars = NumberHelper.ConvertRound(item.Stars);
                item.Code = course?.Code;
            }

            methodResult.Result = new StudentReviewSearchModel { Stars = stars, Code = course?.Code, PagingItems = new PagingItemsModel<StudentReviewTypeModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static StudentReviewTypeModel GetStudentReview(IList<StudentModel>? students, StudentReview x, IList<ClassStudentModel>? classStudents)
        {
            var student = students?.FirstOrDefault(y => y.Id == x.StudentId);
            var classStudent = classStudents?.FirstOrDefault(x => x.StudentId == x.StudentId);
            return new StudentReviewTypeModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                CourseId = x.CourseId,
                ReviewType = x.ReviewType,
                StudentId = x.StudentId,
                CodeStudent = student?.Code,
                ClassCode = classStudent?.Code,
                Stars = x.StudentReviewDetails.Any() ? x.StudentReviewDetails.Average(x => x.VoteStars) : default,
                StudentReviewQuestionTypes = x.StudentReviewDetails.Select(x => new StudentReviewQuestionTypeModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    ReviewQuestionType = x.ReviewQuestionType,
                    VoteStars = x.VoteStars,
                }).ToList(),
            };
        }
    }
}
