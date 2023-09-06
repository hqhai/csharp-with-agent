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

        public CreateNotificationCommandHandler(INotificationsRepository notificationsRepository, IMapper mapper, INotificationTypeRepository notificationTypeRepository, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, INotificationRemindRepository notificationRemindRepository)
        {
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
            _notificationTypeRepository = notificationTypeRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _notificationRemindRepository = notificationRemindRepository;
        }

        public async Task<MethodResult<NotificationMessageModel>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationMessageModel> methodResult = new MethodResult<NotificationMessageModel>();

            #region Validation
            NotificationMessage notificationNew = _mapper.Map<NotificationMessage>(request);

            // check null data
            var notificationTypeResult = request.NotificationTypeId != Guid.Empty ? await _notificationTypeRepository.GetByIdAsync(request.NotificationTypeId) : null;
            if (notificationTypeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NotificationTypeId), request.NotificationTypeId);
                return methodResult;
            }

            // Lấy ra notificationType của thông báo đó
            // var notificationTypeResult = notificationType.Result;

            //list User bị tắt thông báo
            var listUserOffNotification = await _notificationRemindRepository.Queryable.Where(x => x.Status == EnumNotificationRemindStatus.Off && x.ObjectId == request.ObjectId).Select(x => x.UserId.ToString()).ToListAsync(cancellationToken);

            // Handle list UserId
            GetUsersByRoleQueryModel roleQuery = new GetUsersByRoleQueryModel();
            List<string> listUserId = new List<string>();
            if (request.Roles != null)
            {
                foreach (var item in request.Roles!)
                {
                    roleQuery.Role = item;
                    var user = await _userService.GetUserByRole(roleQuery);
                    if (user.Content?.Result != null)
                    {
                        var users = user.Content.Result;
                        listUserId.AddRange(users.Select(u => u.Id.ToString()));
                    }
                }

                listUserId = listUserId.Where(id => !listUserOffNotification.Contains(id)).ToList();
            }

            #endregion Validation


            #region Handler

            await _notificationsRepository.ExecuteTransactionAsync(async () =>
            {
                //Save into Database
                notificationNew = _notificationsRepository.Add(notificationNew);
                await _notificationsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);


                //Push notification
                var notificationRealTime = new NotificationMessageModel()
                {
                    UserId = notificationNew.UserId,
                    Template = notificationTypeResult?.Template,
                    ObjectId = notificationNew.ObjectId,
                    Message = notificationNew.Message,
                    UserIds = listUserId
                };

                await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);


                //Return Value
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NotificationMessageModel>(notificationNew);
                return methodResult;
            });

            #endregion
            return methodResult;
        }
    }
}
