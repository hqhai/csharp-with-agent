// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Commands.CourseResultCmd.V1i2;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RetakeCourseResultCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public Guid? UserId { get; set; }
    }

    public class RetakeCourseResultCommandHandler : IRequestHandler<RetakeCourseResultCommand, MethodResult<CourseResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseChangingHistoryRepository _courseChangingHistoryRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;

        public RetakeCourseResultCommandHandler(IMapper mapper,
            AuthContext authContext,
            IMediator mediator,
            IUserService userService,
            SaveUserCourseSettingPublisher saveUserCourseSettingPublisher,
            ICourseResultRepository courseResultRepository,
            ICourseChangingHistoryRepository courseChangingHistoryRepository,
            ITestGroupResultRepository testGroupResultRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _courseResultRepository = courseResultRepository;
            _courseChangingHistoryRepository = courseChangingHistoryRepository;
            _testGroupResultRepository = testGroupResultRepository;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(RetakeCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseResultModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var getUserNavigationResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (!getUserNavigationResult.IsOK || getUserNavigationResult.Result?.CurrentCourseResultId == null)
            {
                return methodResult;
            }

            var userNavigation = getUserNavigationResult.Result;
            if (userNavigation?.Status != EnumNavigateActionStatus.ContinueLearning)
            {
                methodResult.AddErrorBadRequest("User is not in a state to learning.");
                return methodResult;
            }

            var userCourseSettingsResult = await _userService.GetUserCourseSettingsAsync(request.UserId ?? _authContext.CurrentUserId);
            if (!userCourseSettingsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(userCourseSettingsResult));
                return methodResult;
            }

            //var userCourseSettings = userCourseSettingsResult.Content?.Result;
            //if (!userCourseSettings.HasRemainingAttempts(EnumUserCourseType.ResetAndLearnAgain, userNavigation.CurrentCourseResultId))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumChangeLevelErrorCode.RetakesExpired), nameof(userCourseSettings));
            //    return methodResult;
            //}

            var courseResult = await _courseResultRepository.Queryable.Where(x => x.Id == userNavigation.CurrentCourseResultId)
                .Include(x => x.Course)
                .Include(x => x.CourseChangingHistories)
                .FirstOrDefaultAsync(cancellationToken);

            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            courseResult.WorkingStatus = EnumWorkingStatus.InActive;
            await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            var createCourseResult = await _mediator.Send(new SaveCourseResultCommand
            {
                StudentId = student.Id,
                CourseId = courseResult.CourseId
            }, cancellationToken);

            if (createCourseResult?.Result == null || !createCourseResult.IsOK)
            {
                methodResult.AddErrorBadRequest("Cant create a new course");
                return methodResult;
            }

            var activeCourseResults = await _courseResultRepository.Queryable
               .Where(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active)
               .ToListAsync(cancellationToken);

            activeCourseResults.ForEach(cr =>
            {
                if (cr.Id != createCourseResult.Result?.Id)
                {
                    cr.WorkingStatus = EnumWorkingStatus.InActive;
                }
            });

            courseResult.WorkingStatus = EnumWorkingStatus.NotWorking;

            var resetCourseHistory = new CourseChangingHistory
            {
                ToProgramId = courseResult.Course.ProgramId.Value,
                ToLevelId = courseResult.Course.LevelId,
                SelectedLevelId = courseResult.Course.LevelId,
                SelectedProgramId = courseResult.Course.ProgramId,
                StudentId = student.Id,
                FromInfo = new FromInfo
                {
                    LevelId = courseResult.Course.LevelId,
                    ProgramId = courseResult.Course.ProgramId,
                    CourseResultId = courseResult.Id
                },
                ToCourseResultId = createCourseResult.Result.Id,
                Action = EnumChangeCourseAction.OverlapLevel,
                Status = EnumChangingStatus.Completed
            };
            _courseChangingHistoryRepository.Add(resetCourseHistory);
            var relatedHistory = courseResult.CourseChangingHistories
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefault(x => x.PtResultId != null);

            if (relatedHistory == null)
            {
                var relatedPtResult = await _testGroupResultRepository.Queryable
                    .Where(x => x.ProgramId == courseResult.Course.ProgramId)
                    .Include(x => x.CurrentLevel)
                    .ToListAsync(cancellationToken);

                var ptResultWithHighestLevel = relatedPtResult
                    .OrderByDescending(x => x.CurrentLevel != null ? x.CurrentLevel.LevelOrder : 0)
                    .FirstOrDefault();

                resetCourseHistory.PtResultId = ptResultWithHighestLevel?.Id;
            }
            else
            {
                resetCourseHistory.PtResultId = relatedHistory.PtResultId;
            }

            await _courseChangingHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            await _userService.UpdateCourseToStudentAsync(courseResult.CourseId);
            await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
            {
                IsDeduction = true,
                LevelId = courseResult.Course.LevelId,
                Type = EnumUserCourseType.ResetAndLearnAgain,
                UserId = request.UserId ?? _authContext.CurrentUserId
            }, cancellationToken).ConfigureAwait(false);

            methodResult.Result = _mapper.Map<CourseResultModel>(createCourseResult.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
