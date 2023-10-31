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
    using Microsoft.Extensions.Logging;

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
        private readonly ILogger<CreateNotificationCommandHandler> _logger;

        public CreateNotificationCommandHandler(INotificationsRepository notificationsRepository, IMapper mapper, INotificationTypeRepository notificationTypeRepository, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, INotificationRemindRepository notificationRemindRepository, IOneSignalProvider oneSignalProvider, ILogger<CreateNotificationCommandHandler> logger)
        {
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
            _notificationTypeRepository = notificationTypeRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _notificationRemindRepository = notificationRemindRepository;
            _oneSignalProvider = oneSignalProvider;
            _logger = logger;
        }

        public async Task<MethodResult<NotificationMessageModel>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationMessageModel> methodResult = new MethodResult<NotificationMessageModel>();

            #region Validation

            NotificationMessage notificationNew = _mapper.Map<NotificationMessage>(request);

            // check null data
            var notificationType = await _notificationTypeRepository.GetByIdAsync(request.NotificationTypeId);
            if (notificationType == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NotificationTypeId), request.NotificationTypeId);
                return methodResult;
            }

            //list User bị tắt thông báo
            var listUserOffNotification = await _notificationRemindRepository.Queryable.Where(x => x.Status == EnumNotificationRemindStatus.Off && x.ObjectId == request.ObjectId).Select(x => x.UserId).ToListAsync(cancellationToken);

            // Handle list UserId
            GetUsersByRoleQueryModel roleQuery = new GetUsersByRoleQueryModel();
            List<Guid> listUserId = new List<Guid>();
            if (request.Roles != null)
            {
                foreach (var item in request.Roles)
                {
                    roleQuery.Role = item;
                    var user = await _userService.GetUserByRoleAsync(roleQuery);
                    if (user.Content?.Result != null)
                    {
                        var users = user.Content.Result;
                        listUserId.AddRange(users.Select(u => u.Id));
                    }
                }

                //Loại bỏ những phần tử không có trong listUserId
                listUserId = listUserId.Where(id => !listUserOffNotification.Contains(id)).ToList();
            }

            #endregion Validation

            List<NotificationMessage> listNotificationMessage = new List<NotificationMessage>();
            if (listUserId.Count > 0)
            {
                foreach (var item in listUserId)
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
                else
                {
                    notificationNew = _notificationsRepository.Add(notificationNew);
                }

                await _notificationsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                string avatarPath = string.Empty;

                if (request.SenderId.HasValue)
                {
                    var senderInfo = await _userService.GetUserByIdAsync(request.SenderId.ToString());
                    avatarPath = senderInfo?.Content?.Result?.AvatarPath ?? string.Empty;
                }

                //Push notification to onesignal
                var oneSignalMessage = _mapper.Map<OneSignalMessageModel>(notificationNew);
                oneSignalMessage.UserIds = listUserId;
                oneSignalMessage.AvatarPath = avatarPath;
                oneSignalMessage.Data = new
                {
                    notificationType.Type,
                    notificationType.Content
                };
                await _oneSignalProvider.CreateNotificationAsync(oneSignalMessage, cancellationToken);

                //Push notification to websocket
                var notificationRealTime = _mapper.Map<NotificationMessageModel>(notificationNew);
                notificationRealTime.UserIds = listUserId;
                notificationRealTime.AvatarPath = avatarPath;
                notificationRealTime.Type = notificationType.Type;
                notificationRealTime.Content = notificationType.Content;

                await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);

                //Return Value
                methodResult.Result = _mapper.Map<NotificationMessageModel>(notificationNew);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            #endregion Handler

            return methodResult;
        }
    }
}
