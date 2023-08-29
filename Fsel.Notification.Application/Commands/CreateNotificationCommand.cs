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
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Notification.Application.Queues.Publishers;

    public class CreateNotificationCommand : CreateNotificationCommandModel, IRequest<MethodResult<NotificationsModel>>
    {
    }

    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, MethodResult<NotificationsModel>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly INotificationTypeRepository _notificationTypeRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public CreateNotificationCommandHandler(INotificationsRepository notificationsRepository, IMapper mapper, INotificationTypeRepository notificationTypeRepository, NotificationMessagePublisher notificationMessagePublisher)
        {
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
            _notificationTypeRepository = notificationTypeRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<NotificationsModel>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationsModel> methodResult = new MethodResult<NotificationsModel>();

            #region Validation
            Notifications notificationNew = _mapper.Map<Notifications>(request);

            // check null data

            //var notificationTypeId = request.NotificationTypeId != Guid.Empty ? _notificationTypeRepository.GetByIdAsync(request.NotificationTypeId) : null;
            //if (notificationTypeId == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NotificationTypeId), request.NotificationTypeId);
            //    return methodResult;
            //}

            //var user = request.UserId != Guid.Empty ? await _userService.GetUserById(request.UserId) : null;
            //if (user == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserId), request.UserId);
            //    return methodResult;
            //}

            #endregion Validation


            #region Handler
            await _notificationsRepository.ExecuteTransactionAsync(async () =>
            {
                notificationNew = _notificationsRepository.Add(notificationNew);
                await _notificationsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await _notificationMessagePublisher.Publish(notificationNew, cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NotificationsModel>(notificationNew);
                return methodResult;
            });

            #endregion
            return methodResult;
        }
    }
}
