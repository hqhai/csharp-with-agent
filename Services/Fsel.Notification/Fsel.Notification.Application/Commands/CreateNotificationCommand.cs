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
            var notificationTypeResult = request.NotificationTypeId != Guid.Empty ? await _notificationTypeRepository.GetByIdAsync(request.NotificationTypeId) : null;
            if (notificationTypeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NotificationTypeId), request.NotificationTypeId);
                return methodResult;
            }

            //list User bị tắt thông báo
            var listUserOffNotification = await _notificationRemindRepository.Queryable.Where(x => x.Status == EnumNotificationRemindStatus.Off && x.ObjectId == request.ObjectId).Select(x => x.UserId).ToListAsync(cancellationToken);

            // Handle list UserId
            GetUsersByRoleQueryModel roleQuery = new GetUsersByRoleQueryModel();
            string listUserId = "";
            List<Guid> allIds = new List<Guid>();
            if (request.Roles != null)
            {
                foreach (var item in request.Roles)
                {
                    roleQuery.Role = item;
                    var user = await _userService.GetUserByRoleAsync(roleQuery);
                    if (user.Content?.Result != null)
                    {
                        var userIds = user.Content.Result;
                        allIds.AddRange(userIds.Select(u => u.Id));
                    }
                }

                listUserId = FilteredListUserTurnOnNotification(allIds, listUserOffNotification);
            }

            // Handle 1 UserId
            List<Guid> userOneElement = new List<Guid>();
            userOneElement.Add(notificationNew.UserId);
            string resultOneElement = FilteredListUserTurnOnNotification(userOneElement, listUserOffNotification);
            notificationNew.UserId = string.IsNullOrEmpty(resultOneElement) ? Guid.Empty : new Guid(resultOneElement);

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
                    ObjectId = notificationNew.ObjectId,
                    Message = notificationNew.Message,
                    UserIds = listUserId
                };

                await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);

                //Return Value
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NotificationsModel>(notificationNew);
                return methodResult;
            });

            #endregion Handler

            return methodResult;
        }

        /// <summary>
        /// Lấy ra những User không tắt Notification
        /// </summary>
        /// <param name="allIds"></param>
        /// <param name="listUserOffNotification"></param>
        /// <returns></returns>
        private static string FilteredListUserTurnOnNotification(List<Guid> allIds, List<Guid> listUserOffNotification)
        {
            string result = "";
            var filteredGuids = allIds.Where(id => !listUserOffNotification.Contains(id)).ToList();

            //Chuỗi UserId được join lại từ List User sau khi lọc
            result = string.Join(",", filteredGuids);

            return result;
        }
    }
}
