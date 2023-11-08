// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentFocusTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentFocusTime;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentFocusTimeCommand : CreateStudentFocusTimeCommandModel, IRequest<MethodResult<StudentFocusTimeModel>>
    {
    }

    public class CreateStudentFocusTimeCommandHandler : IRequestHandler<CreateStudentFocusTimeCommand, MethodResult<StudentFocusTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private const int NUMBER_OF_WEEKDAY = 7;
        private const double DEFAULT_TARGET_TIME = 1800; // 1800s tương ứng với 30p
        private const float Archieve_Point = 1;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public CreateStudentFocusTimeCommandHandler(IMapper mapper, IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService, QuestBoardPublisher questBoardPublisher)
        {
            _mapper = mapper;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _studentRepository = studentRepository;
            _authContext = authContext;
            _systemService = systemService;
            _questBoardPublisher = questBoardPublisher;
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

            var nearestConfigTargetTime = GetNearestConfigTime(_studentFocusTimeRepository, student.Id);

            //Thực hiện các hành động lưu xuống database , gửi lên websocket
            await _studentFocusTimeRepository.ExecuteTransactionAsync(async () =>
            {
                if (studentFocusTime == null)
                {
                    studentFocusTime = _mapper.Map<StudentFocusTime>(request);
                    studentFocusTime.TargetTime = nearestConfigTargetTime;
                    studentFocusTime.StudentId = student.Id;
                    studentFocusTime.IsEstablished = false;

                    _studentFocusTimeRepository.Add(studentFocusTime);
                }
                else
                {
                    // Set TargetTime
                    bool confitionChangeTarget = request.TargetTime != nearestConfigTargetTime && request.TargetTime != 0;

                    if (!studentFocusTime.IsEstablished && confitionChangeTarget)
                    {
                        studentFocusTime.TargetTime = request.TargetTime;
                    }
                    studentFocusTime.IsEstablished = confitionChangeTarget;


                    // Set AccessTime And NumberOfToken
                    var systemConfigMap = systemConfigResult!.FirstOrDefault(x => x.TargetTime == studentFocusTime.TargetTime);
                    studentFocusTime.ExecuteTime = request.ExecuteTime;

                    IList<EnumQuestBoardCategory> categories;


                    switch (DEFAULT_TARGET_TIME)
                    {
                        case (double)EnumQuestBoardFocusMode.FocusModeThirtyMinutes:
                            categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.DoneThirtyMinutesFocusMode };
                            break;
                        case (double)EnumQuestBoardFocusMode.FocusModeSixtyMinutes:
                            categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.DoneSixtyMinutesFocusMode };
                            break;
                        case (double)EnumQuestBoardFocusMode.FocusModeNinetyMinutes:
                            categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.DoneNinetyMinutesFocusMode };
                            break;
                        case (double)EnumQuestBoardFocusMode.FocusModeOneHundredTwentytyMinutes:
                            categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.DoneOneHundredTwentytyMinutesFocusMode };
                            break;
                        case (double)EnumQuestBoardFocusMode.FocusModeOneHundredEightyMinutes:
                            categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.DoneOneHundredEightyMinutesFocusMode };
                            break;

                    }

                    if(request.ExecuteTime >= DEFAULT_TARGET_TIME)
                    {
                        await DoQuestBoard(cancellationToken, student ,categories);
                    }    

                 
                    if (studentFocusTime.ExecuteTime >= systemConfigMap!.TargetTime)
                    {
                        student.NumberOfToken += CheckStudentHasStreak(student) ? systemConfigMap.Token * 2 : systemConfigMap.Token;  // Nếu học sinh có streak thì nhân đôi số token
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


        /// <summary>
        /// Check xem học sinh có chuỗi đăng nhập không
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        private bool CheckStudentHasStreak(Student student)
        {
            bool hasStreak = true;

            var currentDate = DateTime.UtcNow.Date;
            var startDate = currentDate.AddDays(-NUMBER_OF_WEEKDAY).Date; // Ngày bắt đầu từ 7 ngày trước
            var endDate = currentDate.Date;
            var studentFocusTimesCheckQuery = _studentFocusTimeRepository.Queryable
                                        .Where(x => x.StudentId == student.Id && x.CreatedDate.Date >= startDate && x.CreatedDate.Date <= endDate && x.ExecuteTime >= x.TargetTime)
                                        .OrderBy(x => x.CreatedDate.Date)
                                        .ToList();

            for (int i = 1; i <= NUMBER_OF_WEEKDAY; i++)
            {
                var expectedDate = currentDate.AddDays(-i);
                var checkDate = studentFocusTimesCheckQuery.FirstOrDefault(x => x.CreatedDate.Date == expectedDate.Date);

                if (checkDate == null)
                {
                    hasStreak = false;
                    break;
                }
            }

            return hasStreak;
        }

        public async Task DoQuestBoard(CancellationToken cancellationToken, Student student, IList<EnumQuestBoardCategory> categories)
        {

            QuestBoardQueueModel questBoardStudentCommand = new QuestBoardQueueModel
            {
                StudentId = student.Id,
                Categories = categories,
                AchievedPoint = Archieve_Point,
            };

            await _questBoardPublisher.Publish(questBoardStudentCommand, cancellationToken);
        }


        /// <summary>
        /// Lấy cấu hình của ngày gần nhất
        /// </summary>
        /// <param name="systemConfigResult"></param>
        /// <returns></returns>
        private static double GetNearestConfigTime(IStudentFocusTimeRepository studentFocusTimeRepository, Guid? studentId)
        {

            var nearestConfigTargetTime = studentFocusTimeRepository.Queryable.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.StudentId == studentId && x.CreatedDate.Date != DateTime.UtcNow.Date)?.TargetTime ?? DEFAULT_TARGET_TIME;

            return nearestConfigTargetTime;
        }

    }
}
