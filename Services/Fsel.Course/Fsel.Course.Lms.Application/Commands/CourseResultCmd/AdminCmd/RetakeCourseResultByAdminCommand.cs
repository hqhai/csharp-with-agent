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
    using Fsel.Course.Lms.Application.Commands.CourseCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;

    public class RetakeCourseResultByAdminCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid UserId { get; set; }
    }

    public class RetakeCourseResultCommandHandler : IRequestHandler<RetakeCourseResultByAdminCommand, MethodResult<CourseResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<RetakeCourseResultByAdminCommand> _logger;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RetakeCourseResultCommandHandler(IMapper mapper
            , ITrainingService trainingService
            , AuthContext authContext
            , IMediator mediator
            , IUserService userService
            , ICourseRepository courseRepository
            , ILogger<RetakeCourseResultByAdminCommand> logger
            , ICourseResultRepository courseResultRepository
            , SaveUserCourseSettingPublisher saveUserCourseSettingPublisher
            , IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _trainingService = trainingService;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
            _courseRepository = courseRepository;
            _logger = logger;
            _courseResultRepository = courseResultRepository;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(RetakeCourseResultByAdminCommand request, CancellationToken cancellationToken)
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
            if (request.CourseLevel != student.CourseLevel)
            {
                methodResult.AddErrorBadRequest(nameof(EnumChangeLevelErrorCode.RetakeNotSameLevel), nameof(request.CourseLevel));
                return methodResult;
            }

            if (!student.CourseId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseId), student.CourseId);
                return methodResult;
            }

            var userCourseSettingsResult = await _userService.GetUserCourseSettingsAsync(_authContext.CurrentUserId);
            if (!userCourseSettingsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(userCourseSettingsResult));
                return methodResult;
            }

            var userCourseSettings = userCourseSettingsResult.Content?.Result;
            if (!userCourseSettings.HasRemainingAttempts(EnumUserCourseType.ResetAndLearnAgain, request.CourseLevel))
            {
                methodResult.AddErrorBadRequest(nameof(EnumChangeLevelErrorCode.RetakesExpired), nameof(userCourseSettings));
                return methodResult;
            }
            var courseResult = await _courseResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                                      .Where(x => x.Course != null && x.Course.CourseLevel == request.CourseLevel)
                                                                      .OrderByDescending(x => x.CreatedDate)
                                                                      .FirstOrDefaultAsync(cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            var course = await GetCourseAsync(request.CourseLevel);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var method = await GetAndCreateCourseResultAsync(course, student, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var classResult = await _trainingService.RegisterClassAsync(new RegisterClassCommandModel
            {
                CourseId = method.Result?.CourseId ?? default,
                UserId = _authContext.CurrentUserId,
            });

            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classResult));
                return methodResult;
            }

            await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
            {
                IsDeduction = true,
                CourseLevel = request.CourseLevel,
                Type = EnumUserCourseType.ResetAndLearnAgain,
                UserId = _authContext.CurrentUserId
            }, cancellationToken).ConfigureAwait(false);

            await _userService.UpdateCourseToStudentAsync(method.Result?.CourseId ?? default);
            methodResult.Result = _mapper.Map<CourseResultModel>(method.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
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

        private async Task<MethodResult<CourseResult>> GetAndCreateCourseResultAsync(Course course, StudentModel student, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseResult>();
            await UpdateCourseActiveToInActiveAsync(student, cancellationToken);
            CourseResult courseResultNew = new CourseResult
            {
                StudentId = student.Id,
                Status = EnumResultStatus.New,
                WorkingStatus = EnumWorkingStatus.Active
            };

            var isCourseResult = await _courseResultRepository.Queryable.AnyAsync(x => x.CourseId == course.Id && x.StudentId == student.Id, cancellationToken);
            if (!isCourseResult)
            {
                await UpdateCoursesStatusNotWorkingAsync(course, student, cancellationToken);
                courseResultNew.CourseId = course.Id;
            }
            else
            {
                var courseClone = await GetCourseAsync(course, student, cancellationToken);
                if (courseClone == null)
                {
                    var courseCloneResult = await _mediator.Send(new CloneCourseCommand { CourseId = course.Id }, cancellationToken);
                    if (!courseCloneResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(courseCloneResult.ErrorMessages);
                        return methodResult;
                    }
                    var courseCloneModel = courseCloneResult.Result;
                    courseResultNew.CourseId = courseCloneModel?.Id ?? default;
                }
                else
                {
                    courseResultNew.CourseId = courseClone.Id;
                }
            }
            if (!courseResultNew.IsValid())
            {
                methodResult.AddErrorBadRequest(courseResultNew.ErrorMessages);
                return methodResult;
            }
            try
            {
                await _courseResultRepository.BulkMergeAsync(new List<CourseResult> { courseResultNew }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.IsDeleted };
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate Retake CourseResult : {ex.Message}");
            }

            methodResult.Result = courseResultNew;
            return methodResult;
        }

        private async Task UpdateCourseActiveToInActiveAsync(StudentModel student, CancellationToken cancellationToken)
        {
            var courseResultActive = await _courseResultRepository.Queryable
                .Where(x => x.Course != null)
                .Where(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active).FirstOrDefaultAsync(cancellationToken);
            if (courseResultActive != null)
            {
                courseResultActive.WorkingStatus = EnumWorkingStatus.InActive;
                await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResultActive }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                });
            }
        }

        private async Task<Course?> GetCourseAsync(Course course, StudentModel student, CancellationToken cancellationToken)
        {
            var courseCloneIds = await UpdateCoursesStatusNotWorkingAsync(course, student, cancellationToken);
            var courseClone = await _courseRepository.Queryable.Where(x => x.ParentCourseId == course.Id && !courseCloneIds.Contains(x.Id))
                                                               .OrderBy(x => x.Priority)
                                                               .FirstOrDefaultAsync(cancellationToken);
            return courseClone;
        }

        private async Task<IList<Guid>> UpdateCoursesStatusNotWorkingAsync(Course course, StudentModel student, CancellationToken cancellationToken)
        {
            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => x.Course != null && x.Course.CourseLevel == course.CourseLevel && x.StudentId == student.Id).ToListAsync(cancellationToken);
            await _courseResultRepository.BulkUpdateList(courseResults.Select(x =>
            {
                x.WorkingStatus = EnumWorkingStatus.NotWorking;
                return x;
            }).ToList(), bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
            });
            return courseResults.Select(x => x.CourseId).ToList();
        }
    }
}
