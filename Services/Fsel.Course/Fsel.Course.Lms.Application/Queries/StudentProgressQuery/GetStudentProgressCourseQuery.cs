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

    public class GetStudentProgressCourseQuery : IRequest<MethodResult<CourseStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressCourseQueryHandler : IRequestHandler<GetStudentProgressCourseQuery, MethodResult<CourseStudentProgressModel>>
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
        private readonly ILearningService _learningService;

        public GetStudentProgressCourseQueryHandler(IUserService userService, ManagerProgressHelper managerProgressHelper, ICourseResultRepository courseResultRepository, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService, ICategoryRepository categoryRepository, ILevelRepository levelRepository, MediatR.IMediator mediator, ILearningService learningService)
        {
            _userService = userService;
            _managerProgressHelper = managerProgressHelper;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _mediator = mediator;
            _learningService = learningService;
        }

        public async Task<MethodResult<CourseStudentProgressModel>> Handle(GetStudentProgressCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseStudentProgressModel> methodResult = new MethodResult<CourseStudentProgressModel>();

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

            var studentId = student.Id;

            var userId = student.UserId;

            var query = await (from cr in _courseResultRepository.Queryable
                               join c in _courseRepository.Queryable on cr.CourseId equals c.Id
                               join p in _categoryRepository.Queryable on c.ProgramId equals p.Id
                               join l in _levelRepository.Queryable on c.LevelId equals l.Id
                               where cr.StudentId == request.StudentId && cr.CourseId == request.CourseId
                               select new
                               {
                                   CourseResult = cr,
                                   Course = c,
                                   Program = p,
                                   Level = l,
                                   Subject = p.CategoryParent
                               }).FirstOrDefaultAsync(cancellationToken);

            if (query == null)
            {
                return methodResult;
            }

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = new List<FeatureAccessTimeQueryModel>
                {
                    new FeatureAccessTimeQueryModel
                    {
                        CourseId = request.CourseId,
                        UserId = userId
                    },
                },
                UserId = userId
            });

            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }

            var featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();

            var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.CourseId == query.Course.Id);

            var learningService = await _learningService.GetLearningTreeFromCourseToTest(request.StudentId, request.CourseId, null, cancellationToken);

            var units = learningService?.Children;
            var lessons = learningService?.Children.SelectMany(p => p.Children).ToList();

            var totalContentCompleted = lessons?.Sum(p => p.TotalContentCompleted) ?? 0;
            var totalContent = lessons?.Sum(p => p.TotalContent) ?? 0;

            var courseProgress = new CourseStudentProgressModel()
            {
                CreatedDate = query.CourseResult.CreatedDate,
                UpdatedDate = query.CourseResult.UpdatedDate,
                CourseId = query.Course.Id,
                CourseName = query.Course.Name,
                StartDate = query.CourseResult.ProcessDate,
                EndDate = query.CourseResult.Status == EnumResultStatus.Done ? (query.CourseResult.CompletionDate ?? query.CourseResult.UpdatedDate) : null,
                Program = query.Program.Name,
                ProgramId = query.Program.Id,
                Level = query.Level.Name,
                LevelId = query.Level.Id,
                Subject = query.Subject.Name,
                SubjectId = query.Subject.Id,
                TimeSpent = featureAccessTime?.AccessTime ?? default,
                Visit = featureAccessTime?.Visit ?? default,
                ContentCompleted = $"{totalContentCompleted} / {totalContent}",
                CurrentUnit = units?.FirstOrDefault(p => p.Status == EnumResultStatus.New || p.Status == EnumResultStatus.Process)?.DisplayOrder,
                CurrentLesson = lessons?.FirstOrDefault(p => p.Status == EnumResultStatus.New || p.Status == EnumResultStatus.Process)?.DisplayOrder
            };

            methodResult.Result = courseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
