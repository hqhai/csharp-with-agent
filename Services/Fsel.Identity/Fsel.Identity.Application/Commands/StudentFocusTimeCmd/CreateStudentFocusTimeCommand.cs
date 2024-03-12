// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentFocusTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queries.StudentFocusTimeQuery;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentFocusTime;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentFocusTimeCommand : CreateStudentFocusTimeCommandModel, IRequest<MethodResult<StudentFocusTimeModel>>
    {
    }

    public class CreateStudentFocusTimeCommandHandler : IRequestHandler<CreateStudentFocusTimeCommand, MethodResult<StudentFocusTimeModel>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;

        public CreateStudentFocusTimeCommandHandler(IMediator mediator, IMapper mapper, CreateTokenHistoryPublisher createTokenHistoryPublisher, ILmsCourseService lmsCourseService, IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService, QuestBoardPublisher questBoardPublisher,
            ITrainingService trainingService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _lmsCourseService = lmsCourseService;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _studentRepository = studentRepository;
            _authContext = authContext;
            _systemService = systemService;
            _questBoardPublisher = questBoardPublisher;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<StudentFocusTimeModel>> Handle(CreateStudentFocusTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFocusTimeModel> methodResult = new MethodResult<StudentFocusTimeModel>();

            var student = _studentRepository.Queryable.Include(x => x.Human).FirstOrDefault(x => x.Human!.UserId == _authContext.CurrentUserId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }
            var studentFocusTime = _studentFocusTimeRepository.Queryable.FirstOrDefault(x => x.StudentId == student.Id && x.CreatedDate.Date == DateTime.UtcNow.Date);

            var systemConfig = await _systemService.GetFocusTimeConfig();
            var systemConfigResult = systemConfig?.Content?.Result;
            if (systemConfigResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            //Thực hiện các hành động lưu xuống database , gửi lên websocket
            await _studentFocusTimeRepository.ExecuteTransactionAsync(async () =>
            {
                if (studentFocusTime == null)
                {
                    studentFocusTime = _mapper.Map<StudentFocusTime>(request);
                    studentFocusTime.TargetTime = request.TargetTime;
                    studentFocusTime.StudentId = student.Id;
                    studentFocusTime.IsEstablished = false;

                    _studentFocusTimeRepository.Add(studentFocusTime);
                }
                else
                {
                    // Set targetTime
                    bool confitionChangeTarget = studentFocusTime.TargetTime == 0 && request.TargetTime != 0;

                    if (!studentFocusTime.IsEstablished && confitionChangeTarget)
                    {
                        studentFocusTime.TargetTime = request.TargetTime;
                        studentFocusTime.IsEstablished = confitionChangeTarget;
                    }

                    // Set AccessTime And NumberOfToken
                    var systemConfigMap = systemConfigResult!.FirstOrDefault(x => x.TargetTime == studentFocusTime.TargetTime);
                    studentFocusTime.ExecuteTime = request.ExecuteTime;

                    if
                    (
                      systemConfigMap != null &&
                      studentFocusTime.ExecuteTime >= systemConfigMap!.TargetTime &&
                      studentFocusTime.IsEstablished
                    )
                    {
                        var courseResult = await _lmsCourseService.GetCourseStudied();
                        var course = courseResult.Content?.Result;

                        if (!courseResult.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                            return methodResult;
                        }
                        var tokenConfig = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
                        {
                            Feature = EnumTokenFeature.FocusMode,
                            Mission = EnumTokenMission.FocusMode,
                            CourseType = course?.CourseType
                        });
                        var tokenConfigResult = tokenConfig.Content?.Result;

                        // làm nhiệm vụ
                        // await DoQuestBoard(student, request.ExecuteTime, studentFocusTime.TargetTime, cancellationToken);

                        var checkSuperFireMode = await _mediator.Send(new CheckSuperFireModeQuery());
                        var isSuperMode = checkSuperFireMode.Result;

                        var tokenConfigFocusModes = tokenConfigResult.GetTokenConfig<IList<TokenConfigFocusModes>>();
                        var targetNumber = tokenConfigFocusModes?.Where(x => x.FocusTimeId == systemConfigMap.Id)?.Max(x => x.BaseValue);

                        if (targetNumber.HasValue && !studentFocusTime.IsReceivedToken && tokenConfigResult != null)
                        {
                            await _createTokenHistoryPublisher.Publish(new TokenHistoryQueueModel
                            {
                                ObjectId = studentFocusTime.Id,
                                InitialToken = student.NumberOfToken,
                                RemainToken = targetNumber.Value,
                                VolatileToken = student.NumberOfToken + targetNumber.Value,
                                TokenConfigId = tokenConfigResult.Id,
                                Type = EnumTokenHistoryType.Earn,
                                UserId = _authContext.CurrentUserId,
                            }, cancellationToken).ConfigureAwait(false);

                            student.NumberOfToken += targetNumber.Value;
                            studentFocusTime.IsReceivedToken = true;
                        }
                        _studentRepository.Update(student);
                        await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    _studentFocusTimeRepository.Update(studentFocusTime);
                }

                await _studentFocusTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentFocusTimeModel>(studentFocusTime);
                return methodResult;
            });

            return methodResult;
        }

        public async Task DoQuestBoard(Student student, double executeTime, double targetTime, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(student);
            var classModel = await _trainingService.GetClassByStudentId(student.Id);
            var courseId = classModel.Content!.Result!.CourseId;

            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>();
            var categoryToElement = EnumQuestBoardCategory.ThirtyMinutesFocusMode;
            switch (targetTime)
            {
                case (double)EnumQuestBoardFocusMode.FocusModeThirtyMinutes:
                    categoryToElement = EnumQuestBoardCategory.ThirtyMinutesFocusMode;
                    break;

                case (double)EnumQuestBoardFocusMode.FocusModeSixtyMinutes:
                    categoryToElement = EnumQuestBoardCategory.SixtyMinutesFocusMode;
                    break;

                case (double)EnumQuestBoardFocusMode.FocusModeNinetyMinutes:
                    categoryToElement = EnumQuestBoardCategory.NinetyMinutesFocusMode;
                    break;

                case (double)EnumQuestBoardFocusMode.FocusModeOneHundredTwentytyMinutes:
                    categoryToElement = EnumQuestBoardCategory.OneHundredTwentytyMinutesFocusMode;
                    break;

                case (double)EnumQuestBoardFocusMode.FocusModeOneHundredEightyMinutes:
                    categoryToElement = EnumQuestBoardCategory.OneHundredEightyMinutesFocusMode;
                    break;
            };
            categories.Add(categoryToElement);

            QuestBoardQueueModel questBoardQueueModel = new QuestBoardQueueModel
            {
                StudentId = student.Id,
                Categories = categories,
                AchievedPoint = ValueSettings.QuestBoardPoint.Achieved_Point,
                CourseId = courseId
            };

            if (executeTime >= targetTime)
            {
                questBoardQueueModel.Categories.Add(EnumQuestBoardCategory.FinishDailyFocusMode); // Mốc hoàn thành focusmode hàng ngày
                await _questBoardPublisher.Publish(questBoardQueueModel, cancellationToken);
            }
        }
    }
}
