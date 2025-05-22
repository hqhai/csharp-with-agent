// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SendMailReminderPTAndKickOffEventCommandModel
    {
        public string? EventCode { get; set; }
        public string? Subject { get; set; }
        public ICollection<Guid>? StudentIds { get; set; }
        public IFormFile? File { get; set; }
        public bool IsDonePT { get; set; }
    }

    public class SendMailReminderPTAndKickOffEventCommand : SendMailReminderPTAndKickOffEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendMailReminderPTAndKickOffEventCommandHandler : IRequestHandler<SendMailReminderPTAndKickOffEventCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IMediator _mediator;

        public SendMailReminderPTAndKickOffEventCommandHandler(IUserService userService, IPlacementTestResultRepository placementTestResultRepository, IMediator mediator)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SendMailReminderPTAndKickOffEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.EventCode) || string.IsNullOrEmpty(request.Subject) || request.File == null || request.File.Length == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var studentIds = new List<Guid>();
            var students = new List<StudentModel>();

            if (request.StudentIds != null && request.StudentIds.Count > 0)
            {
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(request.StudentIds.ToList());
                if (!studentResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResults.Error);
                    return methodResult;
                }
                students = studentResults.Content?.Result?.ToList();
            }
            else
            {
                var studentResults = await _userService.GetStudentsByEventCode(request.EventCode);
                if (!studentResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResults.Error);
                    return methodResult;
                }
                students = studentResults.Content?.Result?.ToList();
            }

            if (students == null || students.Count == 0)
            {
                return methodResult;
            }

            studentIds = students.Select(p => p.Id).ToList();

            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && studentIds.Contains(x.StudentId))
                                                                              .GroupBy(x => x.StudentId).Select(p => new
                                                                              {
                                                                                  StudentId = p.Key,
                                                                                  PTStart = p.Select(x => x).OrderBy(n => n.CreatedDate).FirstOrDefault(),
                                                                                  PTEnd = p.Select(x => x).OrderByDescending(n => n.CreatedDate).FirstOrDefault(),
                                                                              }).ToListAsync(cancellationToken);

            var unfinishedPT = new List<StudentModel>();
            var completePT = new List<StudentModel>();

            foreach (var student in students)
            {
                int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday);

                var placementTestResult = placementTestResults.FirstOrDefault(p => p.StudentId == student.Id);

                var placementTestResultLast = placementTestResult?.PTEnd;

                var placementTestResultInitial = placementTestResult?.PTStart;

                var (levelNext, isLock) = placementTestResultLast?.Level.GetLevelInScore(placementTestResultLast.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age)) ?? (null, default);

                if (!isLock)
                {
                    unfinishedPT.Add(student);
                }
                else if (isLock && request.IsDonePT)
                {
                    completePT.Add(student);
                }
            }

            var studentsEvent = !request.IsDonePT ? unfinishedPT : completePT;

            var content = string.Empty;

            using (var stream = request.File.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync(cancellationToken);
            }

            if (string.IsNullOrEmpty(content))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            foreach (var student in studentsEvent)
            {
                var studentInfo = new
                {
                    FullName = student.User?.FullName,
                    Email = student.User?.Email,
                    School = student.School,
                    Code = student.User?.Code,
                    PhoneNumber = student.User?.PhoneNumber,
                    UserName = student.User?.UserName
                };

                var @params = ObjectHelper.GetDictionary(studentInfo);
                var html = content;
                @params.ForEach(item =>
                {
                    html = html.Replace($"[{item.Key}]", item.Value, StringComparison.CurrentCultureIgnoreCase);
                });

                await _mediator.Send(new SenderCommand
                {
                    Email = student.User?.Email,
                    Subject = request.Subject,
                    Content = html
                }, cancellationToken).ConfigureAwait(false);
            }

            return methodResult;
        }
    }
}
