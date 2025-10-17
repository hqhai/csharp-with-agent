using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Lms.Application.Queues.Publishers;
using Fsel.Course.Lms.Application.Services.OrderServices;
using Fsel.Course.Lms.Application.Services.OrderServices.Model;
using Fsel.Course.Lms.Application.Services.TrainingServices;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Course.Lms.Application.Services.UserServices.Models;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.OtherCmd
{
    public class PushNoticeCommand : IRequest<MethodResult<bool>>
    {
        public EnumPushNoticeTimeType TimeNotifyType { get; set; }
    }

    public class PushNoticeCommandHandler : IRequestHandler<PushNoticeCommand, MethodResult<bool>>
    {
        private readonly ITrainingService _trainingService;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IUserService _userService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IOrderService _orderService;

        public PushNoticeCommandHandler(ITrainingService trainingService, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IUserService userService, NotificationMessagePublisher notificationMessagePublisher, IPlacementTestGroupResultRepository placementTestGroupResultRepository, IOrderService orderService)
        {
            _trainingService = trainingService;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _userService = userService;
            _notificationMessagePublisher = notificationMessagePublisher;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _orderService = orderService;
        }

        public async Task<MethodResult<bool>> Handle(PushNoticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var dateTimeUTC = DateTime.UtcNow;
            var dateTimeVietNam = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var studentChooseLevelResults = await _trainingService.GetStudentsIn7DayChooseLevel();
            var studentChooseLevels = studentChooseLevelResults.Content?.Result;

            var studentChooseLevelIds = studentChooseLevels?.Select(p => p.StudentId).ToList();

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var studentDonePTs = await _placementTestGroupResultRepository.Queryable.Where(p => p.CompletionDate.HasValue && p.CompletionDate.Value.Date >= sevenDaysAgo.Date).ToListAsync(cancellationToken);

            var donePT1 = studentDonePTs.Where(p => studentChooseLevelIds == null || !studentChooseLevelIds.Contains(p.StudentId)).ToList();
            var donePT2 = studentDonePTs.Where(p => studentChooseLevelIds != null && studentChooseLevelIds.Contains(p.StudentId)).ToList();

            var userIdsDonePT = await GetUserIdsHasOrderPayment(donePT2.Select(p => p.CreatedUserId).ToList());

            donePT2 = donePT2.Where(p => userIdsDonePT == null || !userIdsDonePT.Contains(p.CreatedUserId)).ToList();

            studentDonePTs = donePT1.Concat(donePT2).OrderByDescending(p => p.CreatedDate).ToList();

            studentDonePTs = studentDonePTs.DistinctBy(p => p.StudentId).ToList();

            var userIdsHasOrderPayment = await GetUserIdsHasOrderPayment(studentChooseLevels?.Select(p => p.UserId).ToList());

            studentChooseLevels = studentChooseLevels?.Where(p => userIdsHasOrderPayment != null && userIdsHasOrderPayment.Contains(p.UserId)).ToList();

            studentChooseLevelIds = studentChooseLevels?.Select(p => p.StudentId).ToList();

            var studentHasTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.WhereBulkContains(studentChooseLevelIds, p => p.StudentId).Select(p => p.StudentId).Distinct().ToListAsync(cancellationToken);

            studentChooseLevels = studentChooseLevels?.Where(p => !studentHasTimeCodeResults.Contains(p.StudentId)).ToList();

            studentChooseLevels = studentChooseLevels?.OrderByDescending(p => p.UserId).ToList();

            studentChooseLevels = studentChooseLevels?.DistinctBy(p => p.UserId).ToList();

            studentChooseLevelIds = studentChooseLevels?.Select(p => p.StudentId).ToList();

            if (studentChooseLevels == null && studentDonePTs == null)
            {
                return methodResult;
            }

            if (request.TimeNotifyType == EnumPushNoticeTimeType.EveryHour)
            {
                var userIds = new List<Guid>();

                var studentEvents = await GetStudentIdsInEvent(studentChooseLevelIds);
                if (studentEvents != null && studentEvents.Any())
                {
                    studentChooseLevels = studentChooseLevels?.Where(p => studentEvents.Contains(p.StudentId)).ToList();
                    studentChooseLevels = studentChooseLevels?.Where(p => p.CreatedDate >= dateTimeUTC.AddHours(-2) && p.CreatedDate <= dateTimeUTC.AddHours(-1)).ToList();
                    userIds = studentChooseLevels?.Select(p => p.UserId).ToList();
                    await SendNotification(userIds, EnumNotificationContent.OneHourAfterChooseLevel, cancellationToken);
                }

                studentDonePTs = studentDonePTs?.Where(p => p.CompletionDate.HasValue && p.CompletionDate >= dateTimeUTC.AddHours(-2) && p.CompletionDate <= dateTimeUTC.AddHours(-1)).ToList();
                userIds = studentDonePTs?.Select(p => p.CreatedUserId).ToList();
                await SendNotification(userIds, EnumNotificationContent.OneHourAfterDonePT, cancellationToken);
            }
            else if (request.TimeNotifyType == EnumPushNoticeTimeType.At07h30)
            {
                var day1 = GetStudentsChooseLevel(studentChooseLevels, 1, dateTimeVietNam);
                await SendNotification(day1, EnumNotificationContent.Day1At7h30AfterChooseLevel, cancellationToken);

                day1 = GetUsers(studentDonePTs, 1, dateTimeVietNam);
                await SendNotification(day1, EnumNotificationContent.Day1At7h30AfterDonePT, cancellationToken);

                var day3 = GetStudentsChooseLevel(studentChooseLevels, 3, dateTimeVietNam);
                await SendNotification(day3, EnumNotificationContent.Day3At7h30AfterChooseLevel, cancellationToken);

                day3 = GetUsers(studentDonePTs, 3, dateTimeVietNam);
                await SendNotification(day3, EnumNotificationContent.Day3At7h30AfterDonePT, cancellationToken);

                var day4 = GetUsers(studentDonePTs, 4, dateTimeVietNam);
                await SendNotification(day4, EnumNotificationContent.Day4At7h30AfterDonePT, cancellationToken);

                var day7 = GetStudentsChooseLevel(studentChooseLevels, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At7h30AfterChooseLevel, cancellationToken);

                day7 = GetUsers(studentDonePTs, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At7h30AfterDonePT, cancellationToken);
            }
            else if (request.TimeNotifyType == EnumPushNoticeTimeType.At12h00)
            {
                var day2 = GetStudentsChooseLevel(studentChooseLevels, 2, dateTimeVietNam);
                await SendNotification(day2, EnumNotificationContent.Day2At12h00AfterChooseLevel, cancellationToken);

                day2 = GetUsers(studentDonePTs, 2, dateTimeVietNam);
                await SendNotification(day2, EnumNotificationContent.Day2At12h00AfterDonePT, cancellationToken);

                var studentEventResults = await GetStudentIdsInEvent(studentChooseLevelIds);
                if (studentEventResults != null && studentEventResults.Any())
                {
                    var studentEvents = studentChooseLevels?.Where(p => studentEventResults.Contains(p.StudentId)).ToList();
                    var day3 = GetStudentsChooseLevel(studentEvents, 3, dateTimeVietNam);
                    await SendNotification(day3, EnumNotificationContent.Day3At12h00AfterChooseLevel, cancellationToken);
                }

                var day3i1 = GetUsers(studentDonePTs, 3, dateTimeVietNam);
                await SendNotification(day3i1, EnumNotificationContent.Day3At12h00AfterDonePT, cancellationToken);

                var day5 = GetStudentsChooseLevel(studentChooseLevels, 5, dateTimeVietNam);
                await SendNotification(day5, EnumNotificationContent.Day5At12h00AfterChooseLevel, cancellationToken);

                day5 = GetStudents(studentDonePTs, 5, dateTimeVietNam);
                studentEventResults = await GetStudentIdsInEvent(day5);
                if (studentEventResults != null && studentEventResults.Any())
                {
                    var studentEvents = studentDonePTs?.Where(p => studentEventResults.Contains(p.StudentId)).ToList();
                    day5 = studentEvents?.Select(p => p.CreatedUserId).ToList();
                    await SendNotification(day5, EnumNotificationContent.Day5At12h00AfterDonePT, cancellationToken);
                }

                var day6 = GetStudentsChooseLevel(studentChooseLevels, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At12h00AfterChooseLevel, cancellationToken);

                day6 = GetUsers(studentDonePTs, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At12h00AfterDonePT, cancellationToken);

                var day7 = GetStudentsChooseLevel(studentChooseLevels, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At12h00AfterChooseLevel, cancellationToken);

                day7 = GetStudents(studentDonePTs, 7, dateTimeVietNam);
                studentEventResults = await GetStudentIdsInEvent(day7);
                if (studentEventResults != null && studentEventResults.Any())
                {
                    var studentEvents = studentDonePTs?.Where(p => studentEventResults.Contains(p.StudentId)).ToList();
                    day7 = studentEvents?.Select(p => p.CreatedUserId).ToList();
                    await SendNotification(day7, EnumNotificationContent.Day7At12h00AfterDonePT, cancellationToken);
                }
            }
            else if (request.TimeNotifyType == EnumPushNoticeTimeType.At17h30)
            {
                var day3 = GetUsers(studentDonePTs, 3, dateTimeVietNam);
                await SendNotification(day3, EnumNotificationContent.Day3At17h30AfterDonePT, cancellationToken);

                var day4 = GetStudentsChooseLevel(studentChooseLevels, 4, dateTimeVietNam);
                await SendNotification(day4, EnumNotificationContent.Day4At17h30AfterChooseLevel, cancellationToken);

                day4 = GetUsers(studentDonePTs, 4, dateTimeVietNam);
                await SendNotification(day4, EnumNotificationContent.Day4At17h30AfterDonePT, cancellationToken);

                var day6 = GetStudentsChooseLevel(studentChooseLevels, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At17h30AfterChooseLevel, cancellationToken);

                day6 = GetUsers(studentDonePTs, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At17h30AfterDonePT, cancellationToken);

                var day7 = GetStudentsChooseLevel(studentChooseLevels, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At17h30AfterChooseLevel, cancellationToken);

                day7 = GetUsers(studentDonePTs, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At17h30AfterDonePT, cancellationToken);
            }
            else if (request.TimeNotifyType == EnumPushNoticeTimeType.At19h30)
            {
                var day1 = GetStudentsChooseLevel(studentChooseLevels, 1, dateTimeVietNam);
                await SendNotification(day1, EnumNotificationContent.Day1At19h30AfterChooseLevel, cancellationToken);

                day1 = GetStudents(studentDonePTs, 1, dateTimeVietNam);
                var studentEventResults = await GetStudentIdsInEvent(day1);
                if (studentEventResults != null && studentEventResults.Any())
                {
                    var studentEvents = studentDonePTs?.Where(p => studentEventResults.Contains(p.StudentId)).ToList();
                    day1 = studentEvents?.Select(p => p.CreatedUserId).ToList();
                    await SendNotification(day1, EnumNotificationContent.Day1At19h30AfterDonePT, cancellationToken);
                }

                var day2 = GetStudentsChooseLevel(studentChooseLevels, 2, dateTimeVietNam);
                await SendNotification(day2, EnumNotificationContent.Day2At19h30AfterChooseLevel, cancellationToken);

                day2 = GetStudents(studentDonePTs, 2, dateTimeVietNam);
                studentEventResults = await GetStudentIdsInEvent(day2);
                if (studentEventResults != null && studentEventResults.Any())
                {
                    var studentEvents = studentDonePTs?.Where(p => studentEventResults.Contains(p.StudentId)).ToList();
                    day2 = studentEvents?.Select(p => p.CreatedUserId).ToList();
                    await SendNotification(day2, EnumNotificationContent.Day2At19h30AfterDonePT, cancellationToken);
                }

                var day5 = GetUsers(studentDonePTs, 5, dateTimeVietNam);
                await SendNotification(day5, EnumNotificationContent.Day5At19h30AfterDonePT, cancellationToken);

                var day6 = GetStudentsChooseLevel(studentChooseLevels, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At19h30AfterChooseLevel, cancellationToken);

                day6 = GetUsers(studentDonePTs, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At19h30AfterDonePT, cancellationToken);

                var day7 = GetStudentsChooseLevel(studentChooseLevels, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At19h30AfterChooseLevel, cancellationToken);

                day7 = GetUsers(studentDonePTs, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At19h30AfterDonePT, cancellationToken);
            }

            return methodResult;
        }

        private async Task<IList<Guid>> GetStudentIdsInEvent(IList<Guid>? studentIds, int batchSize = 10000)
        {
            if (studentIds == null || studentIds.Count == 0)
            {
                return new List<Guid>();
            }

            var tasks = new List<Task<IList<Guid>>>();

            for (int i = 0; i < studentIds.Count; i += batchSize)
            {
                var batch = studentIds.Skip(i).Take(batchSize).ToList();

                tasks.Add(Task.Run(async () =>
                {
                    var result = await _userService.GetStudentsInEventByStudentIds(new GetStudentIdsInEventByStudentIdsQueryModel
                    {
                        StudentIds = batch
                    });

                    return result.Content?.Result ?? new List<Guid>();
                }));
            }

            var results = await Task.WhenAll(tasks);

            return results.SelectMany(r => r ?? Enumerable.Empty<Guid>()).ToList();
        }

        private async Task<IList<Guid>> GetUserIdsHasOrderPayment(IList<Guid>? userIds, int batchSize = 10000)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return new List<Guid>();
            }

            var tasks = new List<Task<IList<Guid>>>();

            for (int i = 0; i < userIds.Count; i += batchSize)
            {
                var batch = userIds.Skip(i).Take(batchSize).ToList();

                tasks.Add(Task.Run(async () =>
                {
                    var result = await _orderService.GetUsersHasOrderPayment(new GetUsersHasOrderPaymentModel() { UserIds = batch });

                    return result.Content?.Result ?? new List<Guid>();
                }));
            }

            var results = await Task.WhenAll(tasks);

            return results.SelectMany(r => r ?? Enumerable.Empty<Guid>()).ToList();
        }

        private static IList<Guid>? GetStudentsChooseLevel(IList<StudentsIn7DayChooseLevelModel>? students, int day, DateTime dateTimeVietNam)
        {
            return students?.Where(p => p.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == dateTimeVietNam.AddDays(-day).Date).Select(p => p.UserId).ToList();
        }

        private static IList<Guid>? GetUsers(IList<PlacementTestGroupResult>? placementTestGroupResults, int day, DateTime dateTimeVietNam)
        {
            return placementTestGroupResults?.Where(p => p.CompletionDate.HasValue && p.CompletionDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == dateTimeVietNam.AddDays(-day).Date).Select(p => p.CreatedUserId).ToList();
        }

        private static IList<Guid>? GetStudents(IList<PlacementTestGroupResult>? placementTestGroupResults, int day, DateTime dateTimeVietNam)
        {
            return placementTestGroupResults?.Where(p => p.CompletionDate.HasValue && p.CompletionDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == dateTimeVietNam.AddDays(-day).Date).Select(p => p.StudentId).ToList();
        }

        public async Task SendNotification(IList<Guid>? userIds, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            if (userIds != null && userIds.Any())
            {
                await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
                {
                    UserIds = userIds,
                    ObjectId = Guid.NewGuid(),
                    ParamsMessage = null,
                    Type = EnumNotificationType.LinkPage,
                    Content = content,
                    PlatformCode = EnumPlatformCode.LMS
                }, cancellationToken);
            }
        }
    }
}
