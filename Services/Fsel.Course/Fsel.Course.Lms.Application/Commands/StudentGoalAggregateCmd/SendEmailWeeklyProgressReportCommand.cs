// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendEmailWeeklyProgressReportCommand : IRequest<MethodResult<bool>>
    {
        //public EnumSenderTemplate SenderTemplate { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }

    public class SendEmailWeeklyProgressReportCommandHandler : IRequestHandler<SendEmailWeeklyProgressReportCommand, MethodResult<bool>>
    {
        private readonly ISenderService _senderService;
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;
        private readonly IUserService _userService;
        private const string Subject = "[FSEL] Cùng FSEL quay lại đúng nhịp nhé";

        public SendEmailWeeklyProgressReportCommandHandler(ISenderService senderService, IStudentGoalAggregateRepository studentGoalAggregateRepository, IStudentGoalSummaryRepository studentGoalSummaryRepository, IUserService userService)
        {
            _senderService = senderService;
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailWeeklyProgressReportCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                return methodResult;
            }

            var nowUtc = DateTime.UtcNow;
            int diff = ((int)nowUtc.DayOfWeek + 6) % 7;
            var weekStartUtc = nowUtc.Date.AddDays(-diff).Date;
            var weekEndUtc = weekStartUtc.AddDays(7).Date;

            var queryData = await (from baseQ in _studentGoalAggregateRepository.Queryable
                                   join sum in _studentGoalSummaryRepository.Queryable on baseQ.Id equals sum.StudentGoalAggregateId
                                   where sum.StartDate.Date <= weekEndUtc && sum.EndDate.Date >= weekStartUtc
                                   select new StudentGoalAggregateModel
                                   {
                                       Id = baseQ.Id,
                                       ClassName = baseQ.ClassName,
                                       CombinedProgress = baseQ.CombinedProgress,
                                       TotalCompletedLessons = baseQ.TotalCompletedLessons,
                                       CreatedFullName = baseQ.CreatedFullName,
                                       CreatedDate = baseQ.CreatedDate,
                                       CourseType = baseQ.CourseType,
                                       CourseLevel = baseQ.CourseLevel,
                                       CourseId = baseQ.CourseId,
                                       ConsecutiveBehindWeeks = baseQ.ConsecutiveBehindWeeks,
                                       CompletedLessons = sum.CompletedLessons, // tổng số lesson đã hoàn thành trong tuần
                                       CreatedUserId = baseQ.CreatedUserId,
                                       StudentId = baseQ.StudentId,
                                       UpdatedDate = baseQ.UpdatedDate,
                                       UpdatedFullName = baseQ.UpdatedFullName,
                                       UpdatedUserId = baseQ.UpdatedUserId,
                                       TotalTargetLessons = sum.TotalTargetLessons, // tổng số lượng lesson của course
                                       LessonsPerWeek = sum.LessonsPerWeek, // target tuần
                                   }).ToListAsync(cancellationToken);

            if (queryData.Any())
            {
                queryData = queryData.Where(p => request.StudentIds.Contains(p.StudentId)).ToList();

                var studentIds = queryData.Select(l => l.StudentId).ToList();
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
                var students = studentResults.Content?.Result;

                foreach (var item in queryData)
                {
                    var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                    item.FullName = student?.Human?.FullName;
                    item.Email = student?.Human?.Email;
                }

                var tasks = queryData
                             .Where(p => !string.IsNullOrEmpty(p.Email) && p.Email.IsValidEmail())
                             .Select(p => _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel()
                             {
                                 ToEmails = new List<string> { p.Email ?? string.Empty },
                                 Subject = Subject,
                                 Template = GetSenderTemplate(p.CombinedProgress),
                                 Params = new
                                 {
                                     StudentName = p.FullName,
                                     NumberLesson = p.CompletedLessons,
                                     TargetLesson = p.LessonsPerWeek,
                                     TotalLesson = p.TotalTargetLessons
                                 }
                             }))
                             .ToList();

                await Task.WhenAll(tasks);
            }

            //await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel()
            //{
            //    ToEmails = new List<string> { "nguyenhuukhoa5462@gmail.com" },
            //    Subject = Subject,
            //    Template = request.SenderTemplate,
            //    Params = new
            //    {
            //        StudentName = "Khoa Ozil",
            //        NumberLesson = 1,
            //        TargetLesson = 12,
            //        TotalLesson = 123
            //    }
            //});

            return methodResult;
        }

        private static EnumSenderTemplate GetSenderTemplate(EnumCombinedProgress? combinedProgress)
        {
            switch (combinedProgress)
            {
                case EnumCombinedProgress.TotalAheadWeekAhead:
                    return EnumSenderTemplate.WeeklyProgressReport1;

                case EnumCombinedProgress.TotalAheadWeekOnTrack:
                    return EnumSenderTemplate.WeeklyProgressReport2;

                case EnumCombinedProgress.TotalAheadWeekBehind:
                    return EnumSenderTemplate.WeeklyProgressReport3;

                case EnumCombinedProgress.TotalOnTrackWeekAhead:
                    return EnumSenderTemplate.WeeklyProgressReport4;

                case EnumCombinedProgress.TotalOnTrackWeekOnTrack:
                    return EnumSenderTemplate.WeeklyProgressReport5;

                case EnumCombinedProgress.TotalOnTrackWeekBehind:
                    return EnumSenderTemplate.WeeklyProgressReport6;

                case EnumCombinedProgress.TotalBehindWeekAhead:
                    return EnumSenderTemplate.WeeklyProgressReport7;

                case EnumCombinedProgress.TotalBehindWeekOnTrack:
                    return EnumSenderTemplate.WeeklyProgressReport8;

                case EnumCombinedProgress.TotalBehindWeekBehind:
                    return EnumSenderTemplate.WeeklyProgressReport9;

                default:
                    return EnumSenderTemplate.WeeklyProgressReport1;
            }
        }
    }
}
