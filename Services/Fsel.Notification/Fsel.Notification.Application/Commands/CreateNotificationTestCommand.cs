// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Commands
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Notification.Application.Queues.Publishers;
    using System.Threading;
    using Fsel.Core.Base;

    public class CreateNotificationTestCommand : IRequest<MethodResult<bool>>
    {
        public string? Message { get; set; }
    }

    public class CreateNotificationTestCommandHandler : IRequestHandler<CreateNotificationTestCommand, MethodResult<bool>>
    {
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly AuthContext _authContext;

        public CreateNotificationTestCommandHandler(NotificationMessagePublisher notificationMessagePublisher, AuthContext authContext)
        {
            _notificationMessagePublisher = notificationMessagePublisher;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateNotificationTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            NotificationMessageModel notificationRealTime = new NotificationMessageModel
            {
                Message = request.Message,
                Link = string.Empty,
                UserIds = new List<Guid> { _authContext.CurrentUserId },
                AvatarPath = string.Empty
            };

            await _notificationMessagePublisher.Publish(notificationRealTime, cancellationToken).ConfigureAwait(false);

            //Return Value
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
