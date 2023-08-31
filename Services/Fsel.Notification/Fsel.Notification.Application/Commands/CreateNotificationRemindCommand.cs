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

    public class CreateNotificationRemindCommand : CreateNotificationRemindCommandModel, IRequest<MethodResult<NotificationRemindModel>>
    {
    }

    public class CreateNotificationRemindCommandHandler : IRequestHandler<CreateNotificationRemindCommand, MethodResult<NotificationRemindModel>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationRemindRepository _notificationRemindRepository;
        public CreateNotificationRemindCommandHandler(IMapper mapper, INotificationRemindRepository notificationRemindRepository)
        {
            _mapper = mapper;
            _notificationRemindRepository = notificationRemindRepository;
        }

        public async Task<MethodResult<NotificationRemindModel>> Handle(CreateNotificationRemindCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationRemindModel> methodResult = new MethodResult<NotificationRemindModel>();

            #region Handler
            NotificationRemind notificationNew = _mapper.Map<NotificationRemind>(request);
            await _notificationRemindRepository.ExecuteTransactionAsync(async () =>
            {
                notificationNew = _notificationRemindRepository.Add(notificationNew);
                await _notificationRemindRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                //Return Value
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NotificationRemindModel>(notificationNew);
                return methodResult;
            });

            #endregion
            return methodResult;
        }
    }
}
