// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.LuckyTickets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateLuckyTicketCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class CreateLuckyTicketCommandHandler : IRequestHandler<CreateLuckyTicketCommand, MethodResult<VoidMethodResult>>
    {
        private readonly ILuckyTicketRepository _luckyTicketRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public CreateLuckyTicketCommandHandler(ILuckyTicketRepository luckyTicketRepository, IUserService userService, AuthContext authContext, AppSetting appSetting, NotificationMessagePublisher notificationMessagePublisher)
        {
            _luckyTicketRepository = luckyTicketRepository;
            _userService = userService;
            _authContext = authContext;
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(CreateLuckyTicketCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentsResult.Error);
                return methodResult;
            }

            var student = studentsResult.Content?.Result;

            if (student == null)
            {
                return methodResult;
            }

            if (await _luckyTicketRepository.Queryable.AnyAsync(p => p.LessonResultId == request.LessonResultId && p.StudentId == student.Id, cancellationToken))
            {
                return methodResult;
            }

            var checkLuckySpinResults = await _userService.CheckLuckySpin(_authContext.CurrentUserId);
            if (!checkLuckySpinResults.IsSuccessStatusCode)
            {
                methodResult.AddError(checkLuckySpinResults.Error);
                return methodResult;
            }

            var checkLuckySpin = checkLuckySpinResults.Content?.Result;
            if (checkLuckySpin == null || checkLuckySpin.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var luckyTickets = await _luckyTicketRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Ticket)).Select(p => p.Ticket).ToListAsync(cancellationToken);
            string luckyTicket;

            do
            {
                luckyTicket = Shared.Helpers.NumberHelper.GenerateCodeNumber(6);
            } while (luckyTickets.Contains(luckyTicket));

            await _luckyTicketRepository.ExecuteTransactionAsync(async () =>
            {
                var luckyTicketEntity = new LuckyTicket()
                {
                    LessonResultId = request.LessonResultId,
                    StudentId = student.Id,
                    Ticket = luckyTicket,
                    Status = EnumLuckyTicketStatus.NotWon
                };
                if (!luckyTicketEntity.IsValid())
                {
                    return methodResult;
                }

                _luckyTicketRepository.Add(luckyTicketEntity);
                await _luckyTicketRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;

                await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
                {
                    UserIds = new List<Guid>() { _authContext.CurrentUserId },
                    ObjectId = luckyTicketEntity.Id,
                    ParamsMessage = new List<object> { luckyTicket },
                    Type = EnumNotificationType.LinkPopup,
                    Content = EnumNotificationContent.LuckyTicket,
                    SenderId = _authContext.CurrentUserId,
                    ParamsLink = new List<object> { luckyTicket },
                    PlatformCode = EnumPlatformCode.LMS
                }, cancellationToken);

                return methodResult;
            });
            return methodResult;
        }
    }
}
