// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Commands
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Common.Enums.ErrorCodes;

    public class UpdateNotificationRemindCommand : CreateNotificationRemindCommandModel, IRequest<MethodResult<NotificationRemindModel>>
    {
    }

    public class UpdateNotificationRemindCommandHandler : IRequestHandler<UpdateNotificationRemindCommand, MethodResult<NotificationRemindModel>>
    {
        private readonly IMapper _mapper;
        private readonly INotificationRemindRepository _notificationRemindRepository;
        public UpdateNotificationRemindCommandHandler(IMapper mapper, INotificationRemindRepository notificationRemindRepository)
        {
            _mapper = mapper;
            _notificationRemindRepository = notificationRemindRepository;
        }

        public async Task<MethodResult<NotificationRemindModel>> Handle(UpdateNotificationRemindCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NotificationRemindModel> methodResult = new MethodResult<NotificationRemindModel>();

            var notificationUpdate = await _notificationRemindRepository.Queryable.Where(x => x.UserId == request.UserId && x.ObjectId == request.ObjectId).ToListAsync(cancellationToken);

            NotificationRemind notificationUpdateObject = new NotificationRemind();

            if (notificationUpdate != null && notificationUpdate.Count > 0)
            {
                notificationUpdateObject = notificationUpdate.FirstOrDefault() ?? notificationUpdateObject;
            }

            #region Handler

            await _notificationRemindRepository.ExecuteTransactionAsync(async () =>
            {
                if (notificationUpdateObject != null)
                {
                    notificationUpdateObject = _notificationRemindRepository.Add(notificationUpdateObject);
                    await _notificationRemindRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ObjectId), request.ObjectId);
                }

                //Return Value
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NotificationRemindModel>(notificationUpdateObject);
                return methodResult;
            });

            #endregion
            return methodResult;
        }
    }
}
