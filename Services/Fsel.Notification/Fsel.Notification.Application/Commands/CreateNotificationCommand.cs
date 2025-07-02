// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Commands
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Notification.Application.Queues.Publishers;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Notification.Application.Services;
    using Fsel.Notification.Application.Services.Models;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Common.Models;
    using Fsel.Core.Base.Interfaces;
    using System.Threading;

    public class CreateNotificationCommand : CreateNotificationCommandModel, IRequest<MethodResult<NotificationMessageModel>>
    {
    }

    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, MethodResult<NotificationMessageModel>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly INotificationTypeRepository _notificationTypeRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IUserService _userService;
        private readonly INotificationRemindRepository _notificationRemindRepository;
        private readonly IOneSignalProvider _oneSignalProvider;

        public CreateNotificationCommandHandler(INotificationsRepository notificationsRepository, IMapper mapper, INotificationTypeRepository notificationTypeRepository, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, INotificationRemindRepository notificationRemindRepository, IOneSignalProvider oneSignalProvider)
        {
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
            _notificationTypeRepository = notificationTypeRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _notificationRemindRepository = notificationRemindRepository;
            _oneSignalProvider = oneSignalProvider;
        }

        public async Task<MethodResult<NotificationMessageModel>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationMessageModel> methodResult = new MethodResult<NotificationMessageModel>();
            NotificationMessage notificationNew = _mapper.Map<NotificationMessage>(request);

            #region Validation

            // check null data
            var notificationType = await _notificationTypeRepository.GetByIdAsync(request.NotificationTypeId);
            if (notificationType == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NotificationTypeId), request.NotificationTypeId);
                return methodResult;
            }

            #endregion Validation

            //list user
            List<Guid> userIds = await FilterListUser(request, cancellationToken);
            List<NotificationMessage> listNotificationMessage = new List<NotificationMessage>();
            string avatarPath = string.Empty;

            if (request.SenderId.HasValue)
            {
                var senderInfo = await _userService.GetUserByIdAsync(request.SenderId.ToString());
                avatarPath = senderInfo?.Content?.Result?.AvatarPath ?? string.Empty;
            }

            if (userIds.Count > 0)
            {
                foreach (var item in userIds)
                {
                    NotificationMessage notificationElement = _mapper.Map<NotificationMessage>(request);
                    notificationElement.UserId = item;
                    listNotificationMessage.Add(notificationElement);
                }
            }

            #region Handler

            await _notificationsRepository.ExecuteTransactionAsync(async () =>
            {
                //Save into Database
                if (listNotificationMessage.Count > 0)
                {
                    await _notificationsRepository.AddList(listNotificationMessage);
                }
                await _notificationsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                //Push notification to onesignal
                await PushToOneSignal(notificationType, notificationNew, userIds, avatarPath, cancellationToken);

                //Push notification to websocket
                await PushToWebSocket(notificationNew, userIds, avatarPath, cancellationToken);

                //Return Value
                methodResult.Result = _mapper.Map<NotificationMessageModel>(notificationNew);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            #endregion Handler

            return methodResult;
        }

        /// <summary>
        /// Filter List User nhận thông báo
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<Guid>> FilterListUser(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var listUserOffNotification = await _notificationRemindRepository.Queryable.Where(x => x.Status == EnumNotificationRemindStatus.Off && x.ObjectId == request.ObjectId).Select(x => x.UserId).ToListAsync(cancellationToken);

            GetUsersByRoleQueryModel roleQuery = new GetUsersByRoleQueryModel();
            List<Guid> listUserIds = request.UserIds?.ToList() ?? new List<Guid>();

            if (request.Roles != null)
            {
                foreach (var item in request.Roles)
                {
                    roleQuery.Role = item;
                    var user = await _userService.GetUserByRoleAsync(roleQuery);
                    if (user.Content?.Result != null)
                    {
                        var users = user.Content.Result;
                        listUserIds.AddRange(users.Select(u => u.Id));
                    }
                }
            }

            return listUserIds = listUserIds.Except(listUserOffNotification).ToList();
        }

        /// <summary>
        /// Push notification to websocket
        /// </summary>
        /// <param name="notificationRealTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task PushToWebSocket(NotificationMessage notificationNew, List<Guid> userIds, string avatarPath, CancellationToken cancellationToken)
        {
            var notificationRealTime = _mapper.Map<NotificationMessageModel>(notificationNew);
            notificationRealTime.AvatarPath = avatarPath;
            notificationRealTime.UserIds = userIds;
            await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Push message to onesignal
        /// </summary>
        /// <param name="notificationType"></param>
        /// <param name="notificationNew"></param>
        /// <param name="userIds"></param>
        /// <param name="avatarPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task PushToOneSignal(NotificationType notificationType, NotificationMessage notificationNew, List<Guid> userIds, string avatarPath, CancellationToken cancellationToken)
        {
            var oneSignalMessage = _mapper.Map<OneSignalMessageModel>(notificationNew);
            oneSignalMessage.UserIds = userIds;
            oneSignalMessage.AvatarPath = avatarPath;
            oneSignalMessage.Data = new
            {
                notificationType.Type,
                notificationType.Content
            };

            await _oneSignalProvider.CreateNotificationAsync(oneSignalMessage, cancellationToken);
        }
    }
}
