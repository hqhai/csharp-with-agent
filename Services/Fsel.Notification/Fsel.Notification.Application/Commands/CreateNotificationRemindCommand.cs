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
    using Microsoft.EntityFrameworkCore;

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
            var notificationRemindExists = await _notificationRemindRepository.Queryable.Where(x => x.UserId == request.UserId && x.ObjectId == request.ObjectId).ToListAsync(cancellationToken);

            NotificationRemind notificationUpdateObject = new NotificationRemind();

            if (notificationRemindExists != null && notificationRemindExists.Count > 0)
            {
                notificationUpdateObject = notificationRemindExists.FirstOrDefault() ?? notificationUpdateObject;
            }

            await _notificationRemindRepository.ExecuteTransactionAsync(async () =>
            {
                // Add to database if not Exists
                if (notificationRemindExists == null || notificationRemindExists.Count == 0)
                {
                    NotificationRemind notificationNew = _mapper.Map<NotificationRemind>(request);
                    notificationNew = _notificationRemindRepository.Add(notificationNew);
                    await _notificationRemindRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.Result = _mapper.Map<NotificationRemindModel>(notificationNew);
                }
                // Update database if exists
                else
                {
                    notificationUpdateObject.Status = request.Status;
                    notificationUpdateObject = _notificationRemindRepository.Update(notificationUpdateObject);
                    await _notificationRemindRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.Result = _mapper.Map<NotificationRemindModel>(notificationUpdateObject);
                }
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            #endregion
            return methodResult;
        }
    }
}
