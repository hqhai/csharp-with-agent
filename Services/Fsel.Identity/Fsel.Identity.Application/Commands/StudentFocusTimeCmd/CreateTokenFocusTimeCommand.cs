// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentFocusTimeCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateTokenFocusTimeCommand : IRequest<MethodResult<StudentFocusTimeModel>>
    {
    }

    public class CreateTokenFocusTimeCommandHandler : IRequestHandler<CreateTokenFocusTimeCommand, MethodResult<StudentFocusTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;

        public CreateTokenFocusTimeCommandHandler(IMapper mapper, QuestBoardPublisher questBoardPublisher, ITrainingService trainingService, CreateTokenHistoryPublisher createTokenHistoryPublisher, ILmsCourseService lmsCourseService, IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService)
        {
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
            _trainingService = trainingService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _lmsCourseService = lmsCourseService;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _studentRepository = studentRepository;
            _authContext = authContext;
            _systemService = systemService;
        }

        public async Task<MethodResult<StudentFocusTimeModel>> Handle(CreateTokenFocusTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFocusTimeModel> methodResult = new MethodResult<StudentFocusTimeModel>();

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }
            var studentFocusTimes = await _studentFocusTimeRepository.Queryable.Where(x => x.StudentId == student.Id && x.CreatedDate >= DateTime.UtcNow.AddDays(-1).Date).ToListAsync(cancellationToken);
            var studentFocusTime = studentFocusTimes.Where(x => x.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == DateTime.Now.Date).FirstOrDefault();
            if (studentFocusTime == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentFocusTime));
                return methodResult;
            }

            if (studentFocusTime.IsReceivedToken)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.UserReceivedTokens), nameof(studentFocusTime.IsReceivedToken));
                return methodResult;
            }

            var systemConfig = await _systemService.GetFocusTimeConfig();
            if (!systemConfig.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(systemConfig));
                return methodResult;
            }

            var systemConfigResult = systemConfig?.Content?.Result;
            if (systemConfigResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (studentFocusTime.TargetTime == default && !studentFocusTime.IsEstablished)
            {
                var studentFocusTimeOld = await _studentFocusTimeRepository.Queryable.Where(x => x.StudentId == student.Id && x.TargetTime > 0).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                studentFocusTime.TargetTime = studentFocusTimeOld?.TargetTime ?? default;
            }

            var systemConfigMap = systemConfigResult.FirstOrDefault(x => x.TargetTime == studentFocusTime.TargetTime);
            if (systemConfigMap != null && studentFocusTime.ExecuteTime < systemConfigMap.TargetTime)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.UserNotEnoughTime), nameof(studentFocusTime.ExecuteTime));
                return methodResult;
            }

            double? numberOfToken = default;
            //Thực hiện các hành động lưu xuống database , gửi lên websocket
            await _studentFocusTimeRepository.ExecuteTransactionAsync(async () =>
            {
                if (systemConfigMap != null && studentFocusTime.ExecuteTime >= systemConfigMap.TargetTime)
                {
                    // làm nhiệm vụ
                    // await DoQuestBoard(student, request.ExecuteTime, studentFocusTime.TargetTime, cancellationToken);

                    var courseResult = await _lmsCourseService.GetCourseStudied();
                    if (!courseResult.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                        return methodResult;
                    }
                    var course = courseResult.Content?.Result;
                    var tokenConfig = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
                    {
                        Feature = EnumTokenFeature.FocusMode,
                        Mission = EnumTokenMission.FocusMode,
                        CourseType = course?.CourseType
                    });
                    var tokenConfigResult = tokenConfig.Content?.Result;
                    var tokenConfigFocusModes = tokenConfigResult.GetTokenConfig<IList<TokenConfigFocusModes>>();
                    var targetNumber = tokenConfigFocusModes?.Where(x => x.FocusTimeId == systemConfigMap.Id)?.Max(x => x.BaseValue);
                    numberOfToken = targetNumber;
                    if (targetNumber.HasValue)
                    {
                        await _createTokenHistoryPublisher.Publish(new List<TokenHistoryQueueModel>
                        {
                            new TokenHistoryQueueModel
                            {
                                ObjectId = studentFocusTime.Id,
                                VolatileToken = targetNumber.Value,
                                Type = EnumTokenHistoryType.Recevived,
                                Feature = EnumTokenFeature.FocusMode,
                                Mission = studentFocusTime.TargetTime.GetEnumTokenMission(),
                                UserId = student?.UserId ?? default,
                            }
                        }, cancellationToken).ConfigureAwait(false);
                        studentFocusTime.IsReceivedToken = true;
                    }
                }
                _studentFocusTimeRepository.Update(studentFocusTime);
                await _studentFocusTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var studentFocusTimeModel = _mapper.Map<StudentFocusTimeModel>(studentFocusTime);
                studentFocusTimeModel.NumberOfToken = numberOfToken;

                #region Do QuestBoard

                var days = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow);
                var monDay = days.First();
                var sunDay = days.Last();

                var studentFocusTimes = await _studentFocusTimeRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date && p.IsReceivedToken).ToListAsync();
                if (studentFocusTimes.Count <= 7)
                {
                    await DoQuestBoard(studentFocusTime.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.InfinityFocusMode, cancellationToken);
                    await DoQuestBoard(studentFocusTime.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.CompleteMissionDay, cancellationToken);
                }

                await DoQuestBoard(studentFocusTime.StudentId, EnumQuestBoardType.BeginnerQuests, EnumQuestBoardCategory.CompleteFocusModeFirst, cancellationToken);

                #endregion Do QuestBoard

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = studentFocusTimeModel;
                return methodResult;
            });

            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardType questBoardType, EnumQuestBoardCategory category, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = questBoardType,
                Category = category,
                Value = 1
            }, cancellationToken);
        }
    }
}
