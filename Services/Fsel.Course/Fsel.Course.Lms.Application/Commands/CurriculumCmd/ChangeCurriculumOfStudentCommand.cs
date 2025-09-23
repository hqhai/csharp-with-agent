// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Commands.CourseResultCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class ChangeCurriculumOfStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid CurriculumId { get; set; }
        public Guid? UserId { get; set; }
    }

    public class ChangeCurriculumOfStudentCommandHandler : IRequestHandler<ChangeCurriculumOfStudentCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly ChangeCourseHelper _changeCourseHelper;
        private readonly ICourseRepository _courseRepository;
        private readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        private readonly IUserService _userService;
        private readonly ILogger<ChangeCourseLevelCommand> _logger;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public ChangeCurriculumOfStudentCommandHandler(IMapper mapper, ITrainingService trainingService, AuthContext authContext, ChangeCourseHelper changeCourseHelper, ICourseRepository courseRepository, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IUserService userService, ILogger<ChangeCourseLevelCommand> logger, ICourseResultRepository courseResultRepository, NotificationMessagePublisher notificationMessagePublisher, ICurriculumStudentRepository curriculumStudentRepository, ICurriculumRepository curriculumRepository, ILessonResultRepository lessonResultRepository)
        {
            _mapper = mapper;
            _trainingService = trainingService;
            _authContext = authContext;
            _changeCourseHelper = changeCourseHelper;
            _courseRepository = courseRepository;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _userService = userService;
            _logger = logger;
            _courseResultRepository = courseResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _curriculumStudentRepository = curriculumStudentRepository;
            _curriculumRepository = curriculumRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeCurriculumOfStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var studentResult = await _userService.GetStudentByUserIdAsync(userId);
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var curriculum = await (from baseQuery in _curriculumRepository.Queryable
                                    join cs in _curriculumStudentRepository.Queryable on baseQuery.Id equals cs.CurriculumId
                                    join c in _courseRepository.Queryable on baseQuery.CourseId equals c.Id
                                    join cc in _courseRepository.Queryable on baseQuery.CourseCloneId equals cc.Id
                                    where baseQuery.Id == request.CurriculumId
                                    select new
                                    {
                                        Curriculum = baseQuery,
                                        Course = c,
                                        CourseClone = cc,
                                        CurriculumStudent = cs,
                                    }).FirstOrDefaultAsync(cancellationToken);

            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(curriculum), request.CurriculumId);
                return methodResult;
            }

            if (!student.CourseId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseId), student.CourseId);
                return methodResult;
            }

            if (student.CourseId.Value == curriculum.CourseClone.Id)
            {
                methodResult.AddErrorBadRequest(nameof(EnumChangeLevelErrorCode.DoNotChangeCurrentKey), nameof(request.CurriculumId));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course)
                                   .Where(x => x.Course != null && x.Course.Id == curriculum.CourseClone.Id)
                                   .FirstOrDefaultAsync(x => x.WorkingStatus != EnumWorkingStatus.NotWorking && x.StudentId == student.Id, cancellationToken);

            var course = courseResult?.Course;

            if (course == null)
            {
                course = await GetCourseAsync(curriculum.CourseClone.Id);
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

                methodResult.Result = true;
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
            if (!updateResult.IsSuccessStatusCode)
            {
                methodResult.AddError(updateResult.Error);
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Course?> GetCourseAsync(Guid courseId)
        {
            var course = await _courseRepository.Queryable.Where(x => x.Status == EnumCourseStatus.Active && x.Id == courseId)
                                                        .OrderByDescending(x => x.UpdatedDate)
                                                        .ThenByDescending(x => x.CreatedDate)
                                                        .FirstOrDefaultAsync();
            if (course == null)
            {
                course = await _courseRepository.Queryable.Where(x => x.Status == EnumCourseStatus.InActive && x.Id == courseId)
                                                      .OrderByDescending(x => x.UpdatedDate)
                                                      .ThenByDescending(x => x.CreatedDate)
                                                      .FirstOrDefaultAsync();
            }
            return course;
        }
    }
}
