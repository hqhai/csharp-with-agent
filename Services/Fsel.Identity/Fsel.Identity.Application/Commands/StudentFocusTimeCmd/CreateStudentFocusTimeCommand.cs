// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentFocusTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queries.StudentFocusTimeQuery;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentFocusTime;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Shared.Helpers;

    public class CreateStudentFocusTimeCommand : CreateStudentFocusTimeCommandModel, IRequest<MethodResult<StudentFocusTimeModel>>
    {
    }

    public class CreateStudentFocusTimeCommandHandler : IRequestHandler<CreateStudentFocusTimeCommand, MethodResult<StudentFocusTimeModel>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;

        public CreateStudentFocusTimeCommandHandler(IMediator mediator, IMapper mapper, IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService, QuestBoardPublisher questBoardPublisher,
            ITrainingService trainingService)
        {
            _mediator = mediator;
            _mapper = mapper;
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
                        //var tokenConfig = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
                        //{
                        //    Feature = EnumTokenFeature.FocusMode,
                        //    Mission = EnumTokenMission.FocusTime
                        //});
                        //var tokenConfigResult = tokenConfig.Content?.Result;

                        // làm nhiệm vụ
                        // await DoQuestBoard(student, request.ExecuteTime, studentFocusTime.TargetTime, cancellationToken);

                        var checkSuperFireMode = await _mediator.Send(new CheckSuperFireModeQuery());
                        var isSuperMode = checkSuperFireMode.Result;

                        //var targetConfig = tokenConfigResult.GetTokenNumber<TokenFocusTime>(isSuperMode);
                        //var targetNumber = targetConfig?.FocusTimes?.FirstOrDefault(x => x.FocusTimeId == systemConfigMap.Id)?.Number;

                        //if (targetNumber.HasValue)
                        //{
                        //    student.NumberOfToken += targetNumber.Value;
                        //}
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
