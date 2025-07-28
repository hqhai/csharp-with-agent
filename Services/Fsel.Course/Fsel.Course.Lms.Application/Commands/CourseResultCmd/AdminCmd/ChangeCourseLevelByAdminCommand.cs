// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd.AdminCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;

    public class ChangeCourseLevelByAdminCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public EnumCourseLevel CourseLevel { get; set; }

        public Guid UserId { get; set; }
    }

    public class ChangeCourseLevelCommandHandler : IRequestHandler<ChangeCourseLevelByAdminCommand, MethodResult<CourseResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ChangeCourseLevelByAdminCommand> _logger;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeCourseLevelCommandHandler(IMapper mapper
            , ITrainingService trainingService
            , AuthContext authContext
            , ICourseRepository courseRepository
            , IUserService userService
            , ILogger<ChangeCourseLevelByAdminCommand> logger
            , ICourseResultRepository courseResultRepository
            , NotificationMessagePublisher notificationMessagePublisher
            , IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _trainingService = trainingService;
            _authContext = authContext;
            _courseRepository = courseRepository;
            _userService = userService;
            _logger = logger;
            _courseResultRepository = courseResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(ChangeCourseLevelByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseResultModel> methodResult = new MethodResult<CourseResultModel>();

            var tokenResult = await _userService.GetJWTAsync(request.UserId);
            var authToken = tokenResult.Content?.Result;
            if (authToken != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = "Bearer " + authToken.AccessToken;
                _authContext.CurrentUsername = authToken.FullName;
                _authContext.CurrentUserId = request.UserId;
                _authContext.CurrentFullName = authToken.FullName;
                _authContext.Roles = authToken.Roles;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            if (student.CourseLevel == request.CourseLevel)
            {
                methodResult.AddErrorBadRequest(nameof(EnumChangeLevelErrorCode.DoNotChangeCurrentKey), nameof(request.CourseLevel));
                return methodResult;
            }

            if (!student.CourseId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseId), student.CourseId);
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course)
                                    .Where(x => x.Course != null && x.Course.CourseLevel == request.CourseLevel)
                                    .FirstOrDefaultAsync(x => x.WorkingStatus != EnumWorkingStatus.NotWorking && x.StudentId == student.Id, cancellationToken);
            var course = courseResult?.Course;
            if (course == null)
            {
                course = await GetCourseAsync(request.CourseLevel);
            }
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            await _courseResultRepository.ExecuteTransactionAsync(async () =>
            {
                var courseResultActives = await _courseResultRepository.Queryable.Where(x => x.WorkingStatus == EnumWorkingStatus.Active && x.StudentId == student.Id).ToListAsync(cancellationToken);
                if (courseResultActives != null)
                {
                    foreach (var item in courseResultActives)
                    {
                        item.WorkingStatus = EnumWorkingStatus.InActive;
                    }
                    await _courseResultRepository.BulkUpdateList(courseResultActives, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                    });
                }

                if (courseResult == null)
                {
                    courseResult = new CourseResult
                    {
                        CourseId = course.Id,
                        StudentId = student.Id,
                        Status = EnumResultStatus.New,
                        WorkingStatus = EnumWorkingStatus.Active
                    };

                    if (!courseResult.IsValid())
                    {
                        methodResult.AddErrorBadRequest(courseResult.ErrorMessages);
                        return methodResult;
                    }
                    try
                    {
                        await _courseResultRepository.BulkMergeAsync(new List<CourseResult> { courseResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.IsDeleted };
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Log Duplicate CourseResult : {ex.Message}");
                        courseResult = await _courseResultRepository.Queryable.Where(x => x.WorkingStatus == EnumWorkingStatus.Active && x.StudentId == student.Id)
                                                                              .FirstOrDefaultAsync(cancellationToken) ?? new CourseResult();
                    }
                }
                else
                {
                    courseResult.WorkingStatus = EnumWorkingStatus.Active;
                    if (!courseResult.IsValid())
                    {
                        methodResult.AddErrorBadRequest(courseResult.ErrorMessages);
                        return methodResult;
                    }
                    await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                    });
                }

                methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            var classResult = await _trainingService.RegisterClassAsync(new RegisterClassCommandModel
            {
                CourseId = course.Id,
                UserId = _authContext.CurrentUserId,
            });
            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classResult));
                return methodResult;
            }

            var updateResult = await _userService.UpdateCourseToStudentAsync(course.Id);
            if (updateResult.IsSuccessStatusCode)
            {
                await SendNotification(course, courseResult, cancellationToken);
            }

            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SendNotification(Course course, CourseResult? courseResult, CancellationToken cancellationToken)
        {
            if (courseResult == null)
            {
                return;
            }

            NotificationSendingQueueModel notificationModel = new NotificationSendingQueueModel()
            {
                UserIds = new List<Guid>() { courseResult.CreatedUserId },
                ParamsMessage = new List<object> { course.Name ?? string.Empty, },
                Type = EnumNotificationType.LinkPage,
                Content = EnumNotificationContent.CourseChange,
                PlatformCode = EnumPlatformCode.LMS,
                ObjectId = courseResult.Id,
            };

            await _notificationMessagePublisher.Publish(notificationModel, cancellationToken);
        }

        private async Task<Course?> GetCourseAsync(EnumCourseLevel courseLevel)
        {
            var course = await _courseRepository.Queryable.Where(x => x.Status == EnumCourseStatus.Active && x.CourseLevel == courseLevel)
                                                        .OrderByDescending(x => x.UpdatedDate)
                                                        .ThenByDescending(x => x.CreatedDate)
                                                        .FirstOrDefaultAsync();
            if (course == null)
            {
                course = await _courseRepository.Queryable.Where(x => x.Status == EnumCourseStatus.InActive && x.CourseLevel == courseLevel)
                                                      .OrderByDescending(x => x.UpdatedDate)
                                                      .ThenByDescending(x => x.CreatedDate)
                                                      .FirstOrDefaultAsync();
            }
            return course;
        }
    }
}
