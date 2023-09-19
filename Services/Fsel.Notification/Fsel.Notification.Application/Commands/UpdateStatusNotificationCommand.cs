// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Commands
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Notification.Application.Queues.Publishers;
    using Fsel.Notification.Application.Services;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStatusNotificationCommand : UpdateStatusNotificationCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateStatusNotificationCommandHandler : IRequestHandler<UpdateStatusNotificationCommand, MethodResult<bool>>
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IUserService _userService;

        public UpdateStatusNotificationCommandHandler(INotificationsRepository notificationsRepository, AuthContext authContext, NotificationMessagePublisher notificationMessagePublisher, IUserService userService)
        {
            _notificationsRepository = notificationsRepository;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusNotificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            List<NotificationMessage> listNotificationMessage = new List<NotificationMessage>();

            if (!request.MarkReadAll)
            {
                listNotificationMessage = await _notificationsRepository.Queryable.Where(x => request.NotificationMessageIds.Contains(x.Id) && x.Status == EnumNotificationStatus.Sent).ToListAsync(cancellationToken);
            }
            else
            {
                listNotificationMessage = await _notificationsRepository.Queryable.Where(x => x.UserId == _authContext.CurrentUserId && x.Status == EnumNotificationStatus.Sent).ToListAsync(cancellationToken);
            }

            #region validate
            if (listNotificationMessage == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request), _authContext.CurrentUserId);
                return methodResult;
            }
            #endregion

            #region Handler
            await _notificationsRepository.ExecuteTransactionAsync(async () =>
            {

                foreach (var item in listNotificationMessage)
                {
                    item.Status = EnumNotificationStatus.Read;

                    string avatarPath = string.Empty;

                    if (item.SenderId.HasValue)
                    {
                        var senderInfo = await _userService.GetUserByIdAsync(item.SenderId.ToString());
                        avatarPath = senderInfo?.Content?.Result?.AvatarPath ?? string.Empty;
                    }

                    //Push notification with 
                    var notificationRealTime = new NotificationMessageModel()
                    {
                        UserId = item.UserId,
                        ObjectId = item.ObjectId,
                        Message = item.Message,
                        Link = item.Message,
                        SenderId = item.SenderId,
                        Status = EnumNotificationStatus.Read,
                        AvatarPath = avatarPath,
                    };

                    await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);
                }

                _notificationsRepository.UpdateList(listNotificationMessage);
                await _notificationsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                //Return Value
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            #endregion Handler

            return methodResult;
        }
    }
}
