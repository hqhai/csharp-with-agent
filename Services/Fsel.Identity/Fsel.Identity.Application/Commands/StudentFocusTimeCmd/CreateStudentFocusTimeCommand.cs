// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentFocusTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
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
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;
        private const int Near_Day_Filter = -2;

        public CreateStudentFocusTimeCommandHandler(IMediator mediator, IMapper mapper, ILmsCourseService lmsCourseService, IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService, QuestBoardPublisher questBoardPublisher,
            ITrainingService trainingService)
        {
            _mediator = mediator;
            _mapper = mapper;
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

            if (_authContext.CurrentUserId == Guid.Empty)
            {
                _authContext.CurrentUserId = request.UserId;
            }
            var student = _studentRepository.Queryable.FirstOrDefault(x => x.UserId == _authContext.CurrentUserId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            // Load recent student focus time records and filter in memory
            var recentStudentFocusTimes = _studentFocusTimeRepository.Queryable
                .Where(x => x.StudentId == student.Id && x.CreatedDate >= DateTime.UtcNow.AddDays(Near_Day_Filter))
                .ToList();

            var studentFocusTime = recentStudentFocusTimes
                .FirstOrDefault(x => x.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == currentDate.Date);

            var studentFocusTimeNeareast = recentStudentFocusTimes
                .FirstOrDefault(x => x.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == currentDate.Date.AddDays(-1));

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
                    studentFocusTime.TargetTime = studentFocusTimeNeareast != null ? studentFocusTimeNeareast.TargetTime : request.TargetTime;
                    studentFocusTime.StudentId = student.Id;
                    studentFocusTime.IsEstablished = false;

                    _studentFocusTimeRepository.Add(studentFocusTime);
                }
                else
                {
                    // Set targetTime
                    bool confitionChangeTarget = !studentFocusTime!.IsEstablished && request.TargetTime != 0;
                    if (confitionChangeTarget)
                    {
                        studentFocusTime.TargetTime = request.TargetTime;
                        studentFocusTime.IsEstablished = confitionChangeTarget;
                    }

                    // Set AccessTime And NumberOfToken
                    var systemConfigMap = systemConfigResult!.FirstOrDefault(x => x.TargetTime == studentFocusTime.TargetTime);
                    studentFocusTime.ExecuteTime += request.ExecuteTime;

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

                        if (targetNumber.HasValue && !studentFocusTime.IsReceivedToken)
                        {
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
    }
}
