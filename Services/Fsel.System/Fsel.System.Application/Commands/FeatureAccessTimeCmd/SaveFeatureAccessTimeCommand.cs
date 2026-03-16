// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Commands.QuestBoardCmd;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.Models;
    using Fsel.System.Application.Services.UserServices.Models.QueryModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.FeatureAccessTimes;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveFeatureAccessTimeCommand : SaveFeatureAccessTimeCommandModel, IRequest<MethodResult<FeatureAccessTimeModel>>
    {
    }

    public class SaveFeatureAccessTimeCommandHandler : IRequestHandler<SaveFeatureAccessTimeCommand, MethodResult<FeatureAccessTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private List<FeatureAccessTime> _featureAccessTimes = new List<FeatureAccessTime>();
        private List<FeatureAccessTime> _featureAccessTimesUpdate = new List<FeatureAccessTime>();
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveFeatureAccessTimeCommandHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository, AuthContext authContext, IMediator mediator, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(SaveFeatureAccessTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();

            if (string.IsNullOrEmpty(request.Type) || !request.AccessTime.HasValue)
            {
                return methodResult;
            }

            var headers = _httpContextAccessor.HttpContext?.Request?.Headers;
            if (headers != null && headers.ContainsKey("User-Agent"))
            {
                var headerValue = headers["User-Agent"].ToString();
            }
            var mr = await GetStudentAsync();
            if (!mr.IsOK)
            {
                methodResult.AddErrorBadRequest(mr.ErrorMessages);
                return methodResult;
            }
            var student = mr.Result!;

            EnumFeature featureType = ConvertType(request.Type);
            var featureAccessTimeCheck = await GetLatestFeatureAccessTime(featureType, request.ObjectId, cancellationToken);
            bool isActiveToday = featureAccessTimeCheck != null;
            //focus mode
            if (isActiveToday)
            {
                StudentFocusTimeCommandModel cmd = new StudentFocusTimeCommandModel
                {
                    ExecuteTime = request.AccessTime.Value,
                    TargetTime = 0,
                    UserId = _authContext.CurrentUserId,
                };
                await _userService.SaveFocusTime(cmd).ConfigureAwait(false);
            }

            //Daily checkin
            long existsTime = featureAccessTimeCheck != null ? featureAccessTimeCheck.AccessTime + request.AccessTime.Value : 0;
            if (existsTime >= ValueSettings.StudentDailyStreak.CheckInGoalTime)
            {
                StudentDailyStreakCommandModel cmdDaily = new StudentDailyStreakCommandModel
                {
                    StudentId = student.Id,
                    IsUseShield = false,
                    DailyDate = DateTime.UtcNow,
                    UserId = _authContext.CurrentUserId,
                };
                await _userService.SaveDailyStreak(cmdDaily);
            }

            await _featureAccessTimeRepository.ExecuteTransactionAsync(async () =>
            {
                //Phân tách dữ liệu accesstime theo từng khung giờ
                var accessTimes = GetAccessTimeByRangeHour(DateTime.UtcNow, request.AccessTime);

                foreach (var (time, seconds) in accessTimes)
                {
                    if (featureAccessTimeCheck == null || !IsSameRangeHour(featureAccessTimeCheck))
                    {
                        AddNewFeatureAccessTime(request, seconds, time);
                    }
                    else if (featureAccessTimeCheck != null && request.LessonId == null && !IsSameRangeHour(featureAccessTimeCheck))
                    {
                        AddNewFeatureAccessTime(request, seconds, time);
                    }
                    else if (featureAccessTimeCheck != null && IsSameRangeHour(featureAccessTimeCheck))
                    {
                        UpdateExistingFeatureAccessTime(featureAccessTimeCheck, request, seconds);
                    }
                    else if (featureAccessTimeCheck != null)
                    {
                        UpdateExistingFeatureAccessTime(featureAccessTimeCheck, request, seconds);
                    }
                }

                await _featureAccessTimeRepository.AddList(_featureAccessTimes);
                _featureAccessTimeRepository.UpdateList(_featureAccessTimesUpdate);
                await _featureAccessTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                //methodResult.Result = _mapper.Map<FeatureAccessTimeModel>(featureAccessTime);

                if (TrackableFeatures.Contains(request.Type) && request.AccessTime.HasValue)
                {
                    int minute = DateTimeHelper.ConvertSecondsToMinutes(request.AccessTime.Value);
                    await DoQuestBoard(student.Id, EnumQuestBoardCategory.ExploreTheLearningGalaxy, minute, cancellationToken);
                    await DoQuestBoard(student.Id, EnumQuestBoardCategory.LearningSpaceship, minute, cancellationToken);
                }

                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }

        private async Task<FeatureAccessTime?> GetLatestFeatureAccessTime(
        EnumFeature featureType,
        Guid? objectId,
        CancellationToken ct)
        {
            return await _featureAccessTimeRepository.Queryable
                .Where(x => x.CreatedUserId == _authContext.CurrentUserId &&
                            x.EnumFeature == featureType &&
                            (featureType == EnumFeature.Other || x.ObjectId == objectId))
                .OrderByDescending(x => x.LastVisited)
                .FirstOrDefaultAsync(ct);
        }

        private static readonly HashSet<string> TrackableFeatures = new()
        {
            EnumFeature.VideoLesson.ToString(),
            EnumFeature.HomeWork.ToString(),
            EnumFeature.FullTest.ToString(),
            EnumFeature.SkillTest.ToString(),
            EnumFeature.ClassForum.ToString(),
            EnumFeature.ChatBot.ToString()
        };

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardCategory category, int value, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DoQuestBoardCommand()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = category,
                Value = value
            }, cancellationToken);
        }

        private static EnumFeature ConvertType(string value)
        {
            return Enum.TryParse(value, out EnumFeature result) ? result : EnumFeature.Other;
        }

        private void AddNewFeatureAccessTime(SaveFeatureAccessTimeCommand request, long accessTime, DateTime vistedTime)
        {
            var featureAccessTime = _mapper.Map<FeatureAccessTime>(request);
            featureAccessTime.Visit = 1;
            featureAccessTime.LastVisited = DateTime.UtcNow;
            featureAccessTime.AccessTime = accessTime;
            featureAccessTime.EnumFeature = ConvertType(request.Type!);
            _featureAccessTimes.Add(featureAccessTime);
        }

        private void UpdateExistingFeatureAccessTime(FeatureAccessTime featureAccessTime, SaveFeatureAccessTimeCommand request, long accessTime)
        {
            featureAccessTime.Visit += 1;
            featureAccessTime.AccessTime += accessTime;
            featureAccessTime.LastVisited = DateTime.UtcNow;
            featureAccessTime.EnumFeature = ConvertType(request.Type!);
            _featureAccessTimesUpdate.Add(featureAccessTime);
        }

        private static bool IsSameRangeHour(FeatureAccessTime entity)
        {
            if (!entity.LastVisited.HasValue)
            {
                return false;
            }
            var now = DateTime.UtcNow;
            var last = entity.LastVisited.Value;

            return last.Year == now.Year &&
                   last.Month == now.Month &&
                   last.Day == now.Day &&
                   last.Hour == now.Hour;
        }

        /// <summary>
        /// Tính toán accesstime theo từng khung giờ
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="accessTimeInSeconds"></param>
        /// <returns></returns>
        public static List<(DateTime, long)> GetAccessTimeByRangeHour(DateTime currentTime, long? accessTimeInSeconds)
        {
            List<(DateTime, long)> hourlyAccess = new List<(DateTime, long)>();
            long remainingTime = (long)accessTimeInSeconds!;

            // Tính toán số giây từ currentTime đến đầu giờ hiện tại
            int secondsFromHourStart = currentTime.Minute * 60 + currentTime.Second;

            // Vòng lặp qua các giờ, bắt đầu từ giờ hiện tại và đi ngược về trước
            while (remainingTime > 0)
            {
                long secondsToAllocate;

                if (secondsFromHourStart > 0)
                {
                    // Sử dụng số giây từ đầu giờ đến currentTime cho lần lặp đầu tiên
                    secondsToAllocate = Math.Min(secondsFromHourStart, remainingTime);
                    hourlyAccess.Add((currentTime.AddSeconds(-secondsFromHourStart + secondsToAllocate), secondsToAllocate));
                    secondsFromHourStart = 0; // Đặt lại để các lần lặp sau sử dụng đủ 3600 giây
                }
                else
                {
                    // Mỗi giờ tiếp theo sử dụng đủ 3600 giây hoặc số giây còn lại nếu nhỏ hơn
                    secondsToAllocate = Math.Min(3600, remainingTime);
                    currentTime = currentTime.AddHours(-1); // Lùi về đầu giờ trước
                    hourlyAccess.Add((currentTime, secondsToAllocate));
                }

                remainingTime -= secondsToAllocate;
            }

            hourlyAccess.Reverse(); // Đảo ngược thứ tự để thời gian tăng dần
            return hourlyAccess;
        }
    }
}
