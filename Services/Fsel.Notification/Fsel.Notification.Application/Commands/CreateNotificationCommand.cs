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

    public class CreateNotificationCommand : CreateNotificationCommandModel, IRequest<MethodResult<NotificationsModel>>
    {
    }

    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, MethodResult<NotificationsModel>>
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

        public async Task<MethodResult<NotificationsModel>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationsModel> methodResult = new MethodResult<NotificationsModel>();

            #region Validation
            Notifications notificationNew = _mapper.Map<Notifications>(request);

            // check null data
            var notificationType = request.NotificationTypeId != Guid.Empty ? _notificationTypeRepository.GetByIdAsync(request.NotificationTypeId) : null;
            if (notificationType == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NotificationTypeId), request.NotificationTypeId);
                return methodResult;
            }

            // Lấy ra notificationType của thông báo đó
            var notificationTypeResult = notificationType.Result;


            // Get list UserId by role
            GetUsersByRoleQueryModel roleQuery = new GetUsersByRoleQueryModel();
            List<string> allIds = new List<string>();
            foreach (var item in request.Roles!)
            {
                roleQuery.Role = item;
                var user = await _userService.GetUserByRole(roleQuery);
                if (user.Content?.Result != null)
                {
                    var userIds = user.Content.Result;
                    allIds.AddRange(userIds.Select(u => u.Id.ToString()));
                }
            }
            string concatenatedIds = string.Join(",", allIds);


            //list User bị tắt thông báo
            var listUserOffNotification = await _notificationRemindRepository.Queryable.Where(x => x.Status == EnumNotificationRemindStatus.Off && x.ObjectId == request.ObjectId).Select(x => x.UserId).ToListAsync(cancellationToken);


            List<Guid> splitIds = concatenatedIds.Split(',').Select(Guid.Parse).ToList();

            //So sánh list User với List User tắt thông báo, và không lấy những User tắt thông báo trong list User ban đầu
            var filteredGuids = splitIds.Where(id => !listUserOffNotification.Contains(id)).ToList();

            //Chuỗi UserId được join lại từ List User sau khi lọc
            string filteredConcatenatedGuids = string.Join(",", filteredGuids);


            #endregion Validation


            #region Handler

            await _notificationsRepository.ExecuteTransactionAsync(async () =>
            {
                notificationNew = _notificationsRepository.Add(notificationNew);

                await _notificationsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);


                //Push notification
                var notificationRealTime = new NotificationsModel()
                {
                    UserId = notificationNew.UserId,
                    Template = notificationTypeResult?.Template,
                    Message = notificationNew.Message,
                    UserIds = filteredConcatenatedGuids
                };

                await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);


                //Return Value
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NotificationsModel>(notificationNew);
                return methodResult;
            });

            #endregion
            return methodResult;
        }
    }
}
