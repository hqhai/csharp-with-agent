// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.StudentReviewQuery
{
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReviewStudentsByStudentQuery : IRequest<MethodResult<IList<StudentReviewInfoModel>>>
    {
    }

    public class GetReviewStudentsByStudentQueryHandler : IRequestHandler<GetReviewStudentsByStudentQuery, MethodResult<IList<StudentReviewInfoModel>>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly ICourseService _courseService;

        public GetReviewStudentsByStudentQueryHandler(IStudentReviewRepository studentReviewRepository
            , AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , ITrainingService trainingService
            , ICourseService courseService)
        {
            _studentReviewRepository = studentReviewRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _trainingService = trainingService;
            _courseService = courseService;
        }

        public async Task<MethodResult<IList<StudentReviewInfoModel>>> Handle(GetReviewStudentsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentReviewInfoModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var studentId = studentResult.Content?.Result?.Id;

            var classStudents = await _trainingService.GetClassCourseStudentAsync(studentId ?? default);
            if (!classStudents.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                return methodResult;
            }
            var courseIds = classStudents.Content?.Result?.Select(x => x.CourseId).Distinct().ToList();
            List<CourseModel>? courses = new List<CourseModel>();
            if (courseIds != null && courseIds.Count > 0)
            {
                var courseResults = await _courseService.GetListCourseByIds(courseIds);
                if (!courseResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                    return methodResult;
                }
                courses = courseResults.Content?.Result?.ToList();
            }

            var studentReviews = await _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
                .Where(x => x.StudentId == studentId)
                .Select(x => new StudentReviewInfoModel
                {
                    Id = x.Id,
                    ReviewType = x.ReviewType,
                    CourseId = x.CourseId,
                    StudentId = x.StudentId,
                    StudentReviewDetails = _mapper.Map<IList<StudentReviewDetailModel>>(x.StudentReviewDetails.OrderBy(x => x.CreatedDate))
                }).ToListAsync(cancellationToken: cancellationToken);

            foreach (var studentReview in studentReviews)
            {
                if (courses != null && courses.Count > 0 && studentReview.CourseId.HasValue)
                {
                    var course = courses.FirstOrDefault(x => x.Id == studentReview.CourseId);
                    if (course != null)
                    {
                        studentReview.CourseName = course.Name;
                    }
                }
            }

            methodResult.Result = studentReviews;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
