// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressCoursesQuery : IRequest<MethodResult<IList<CourseStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentProgressCoursesQueryHandler : IRequestHandler<GetStudentProgressCoursesQuery, MethodResult<IList<CourseStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILearningService _learningService;

        public GetStudentProgressCoursesQueryHandler(IUserService userService, ManagerProgressHelper managerProgressHelper, ICourseResultRepository courseResultRepository, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService, ILevelRepository levelRepository, ICategoryRepository categoryRepository, IMediator mediator, IServiceProvider serviceProvider, ILearningService learningService)
        {
            _userService = userService;
            _managerProgressHelper = managerProgressHelper;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
            _levelRepository = levelRepository;
            _categoryRepository = categoryRepository;
            _mediator = mediator;
            _serviceProvider = serviceProvider;
            _learningService = learningService;
        }

        public async Task<MethodResult<IList<CourseStudentProgressModel>>> Handle(GetStudentProgressCoursesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseStudentProgressModel>> methodResult = new MethodResult<IList<CourseStudentProgressModel>>();

            var studentResult = await _userService.GetUserByStudentIdWithCache(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var userId = student.UserId;

            var query = await (from cr in _courseResultRepository.Queryable
                               join c in _courseRepository.Queryable on cr.CourseId equals c.Id
                               join p in _categoryRepository.Queryable on c.ProgramId equals p.Id
                               join l in _levelRepository.Queryable on c.LevelId equals l.Id
                               where cr.StudentId == request.StudentId
                               select new
                               {
                                   CourseResult = cr,
                                   Course = c,
                                   Program = p,
                                   Level = l,
                                   Subject = p.CategoryParent
                               }).ToListAsync(cancellationToken);

            var courseIds = query.Select(p => p.Course.Id).ToList();

            if (courseIds == null || courseIds.Count == 0)
            {
                return methodResult;
            }

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = courseIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    CourseId = x,
                    UserId = userId
                }).ToList(),
                UserId = userId
            });

            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }

            var featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();

            var model = courseIds.Select(p => new GetLearningTreeFromCourseToTestModel()
            {
                StudentId = request.StudentId,
                CourseId = p
            }).ToList();

            var learningServices = await _learningService.GetLearningTreeFromCourseToTest(model, cancellationToken);

            var courseProgress = new List<CourseStudentProgressModel>();

            foreach (var item in query)
            {
                var featureAccessTime = featureAccessTimes?
                    .FirstOrDefault(x => x.CourseId == item.Course.Id);

                var learningService = learningServices.FirstOrDefault(p => p.StudentId == request.StudentId && p.LearningTemplateId == item.Course.Id);
                var totalContentCompleted = learningService?.Children.SelectMany(p => p.Children)?.Sum(p => p.TotalContentCompleted) ?? 0;
                var totalContent = learningService?.Children.SelectMany(p => p.Children)?.Sum(p => p.TotalContent) ?? 0;

                courseProgress.Add(new CourseStudentProgressModel
                {
                    CreatedDate = item.CourseResult.CreatedDate,
                    UpdatedDate = item.CourseResult.UpdatedDate,
                    CourseId = item.Course.Id,
                    CourseName = item.Course.Name,
                    CourseCode = item.Course.Code,
                    StartDate = item.CourseResult.ProcessDate,
                    EndDate = item.CourseResult.Status == EnumResultStatus.Done ? (item.CourseResult.CompletionDate ?? item.CourseResult.UpdatedDate) : null,
                    Program = item.Program.Name,
                    ProgramId = item.Program.Id,
                    Level = item.Level.Name,
                    LevelId = item.Level.Id,
                    Subject = item.Subject.Name,
                    SubjectId = item.Subject.Id,
                    TimeSpent = featureAccessTime?.AccessTime ?? default,
                    Visit = featureAccessTime?.Visit ?? default,
                    ContentCompleted = $"{totalContentCompleted} / {totalContent}"
                });
            }

            methodResult.Result = courseProgress.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
