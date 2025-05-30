using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Lms.Application.Queues.Publishers;
using Fsel.Course.Lms.Application.Services.TrainingServices;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Course.Lms.Application.Services.UserServices.Models;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.OtherCmd
{
    public class SendNotifyAfterChooseLevelCommand : IRequest<MethodResult<bool>>
    {
        public EnumNotifyAfterChooseLevelType NotifyAfterChooseLevelType { get; set; }
    }

    public class SendNotifyAfterChooseLevelCommandHandler : IRequestHandler<SendNotifyAfterChooseLevelCommand, MethodResult<bool>>
    {
        private readonly ITrainingService _trainingService;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IUserService _userService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public SendNotifyAfterChooseLevelCommandHandler(ITrainingService trainingService, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IUserService userService, NotificationMessagePublisher notificationMessagePublisher)
        {
            _trainingService = trainingService;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _userService = userService;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(SendNotifyAfterChooseLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var dateTimeUTC = DateTime.UtcNow;
            var dateTimeVietNam = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var studentResults = await _trainingService.GetStudentsIn7DayChooseLevel();
            var students = studentResults.Content?.Result;

            if (students == null || !students.Any())
            {
                return methodResult;
            }

            var studentIds = students.Select(p => p.StudentId).ToList();

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var studentHasTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.CreatedDate.Date >= sevenDaysAgo.Date).Select(p => p.StudentId).Distinct().ToListAsync(cancellationToken);

            students = students.Where(p => !studentHasTimeCodeResults.Contains(p.StudentId)).ToList();

            studentIds = students.Select(p => p.StudentId).ToList();

            if (request.NotifyAfterChooseLevelType == EnumNotifyAfterChooseLevelType.EveryHour)
            {
                var studentEventResults = await _userService.GetStudentsInEventByStudentIds(new GetStudentIdsInEventByStudentIdsQueryModel() { StudentIds = studentIds });
                var studentEvents = studentEventResults.Content?.Result;
                if (studentEvents == null || !studentEvents.Any())
                {
                    return methodResult;
                }
                students = students.Where(p => studentEvents.Contains(p.StudentId)).ToList();
                students = students.Where(p => p.CreatedDate >= dateTimeUTC.AddHours(-2) && p.CreatedDate <= dateTimeUTC.AddHours(-1)).ToList();
                var userIds = students.Select(p => p.UserId).ToList();
                await SendNotification(userIds, EnumNotificationContent.OneHourAfterPT, cancellationToken);
            }
            else if (request.NotifyAfterChooseLevelType == EnumNotifyAfterChooseLevelType.At07h30)
            {
                var day1 = GetStudents(students, 1, dateTimeVietNam);
                await SendNotification(day1, EnumNotificationContent.Day1At7h30, cancellationToken);

                var day3 = GetStudents(students, 3, dateTimeVietNam);
                await SendNotification(day3, EnumNotificationContent.Day3At7h30, cancellationToken);

                var day7 = GetStudents(students, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At7h30, cancellationToken);
            }
            else if (request.NotifyAfterChooseLevelType == EnumNotifyAfterChooseLevelType.At12h00)
            {
                var day2 = GetStudents(students, 2, dateTimeVietNam);
                await SendNotification(day2, EnumNotificationContent.Day2At12h00, cancellationToken);

                var day3 = GetStudents(students, 3, dateTimeVietNam);
                await SendNotification(day3, EnumNotificationContent.Day3At12h00, cancellationToken);

                var day5 = GetStudents(students, 5, dateTimeVietNam);
                await SendNotification(day5, EnumNotificationContent.Day5At12h00, cancellationToken);

                var day6 = GetStudents(students, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At12h00, cancellationToken);

                var day7 = GetStudents(students, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At12h00, cancellationToken);
            }
            else if (request.NotifyAfterChooseLevelType == EnumNotifyAfterChooseLevelType.At17h30)
            {
                var day4 = GetStudents(students, 4, dateTimeVietNam);
                await SendNotification(day4, EnumNotificationContent.Day4At17h30, cancellationToken);

                var day6 = GetStudents(students, 6, dateTimeVietNam);
                await SendNotification(day6, EnumNotificationContent.Day6At17h30, cancellationToken);

                var day7 = GetStudents(students, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At17h30, cancellationToken);
            }
            else if (request.NotifyAfterChooseLevelType == EnumNotifyAfterChooseLevelType.At19h30)
            {
                var day2 = GetStudents(students, 2, dateTimeVietNam);
                await SendNotification(day2, EnumNotificationContent.Day2At19h30, cancellationToken);

                var day7 = GetStudents(students, 7, dateTimeVietNam);
                await SendNotification(day7, EnumNotificationContent.Day7At19h30, cancellationToken);
            }

            return methodResult;
        }

        private static IList<Guid> GetStudents(IList<StudentsIn7DayChooseLevelModel> students, int day, DateTime dateTimeVietNam)
        {
            return students.Where(p => p.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == dateTimeVietNam.AddDays(-day).Date).Select(p => p.UserId).ToList();
        }

        public async Task SendNotification(IList<Guid> userIds, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                UserIds = userIds,
                ObjectId = Guid.NewGuid(),
                ParamsMessage = null,
                Type = EnumNotificationType.Text,
                Content = content,
                PlatformCode = EnumPlatformCode.LMS
            }, cancellationToken);
        }
    }
}
