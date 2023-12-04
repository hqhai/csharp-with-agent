// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class PaymentSuccessCommand : IRequest<MethodResult<bool>>
    {
        public string? OrderCode { get; set; }
    }

    public class PaymentSuccessCommandHandler : IRequestHandler<PaymentSuccessCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ILmsCourseService _lmsCourseService;

        public PaymentSuccessCommandHandler(IOrderRepository orderRepository, NotificationMessagePublisher notificationMessagePublisher, ILmsCourseService lmsCourseService)
        {
            _orderRepository = orderRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<bool>> Handle(PaymentSuccessCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.OrderCode, cancellationToken);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            //var courseResults = await _lmsCourseService.GetCoursesByIdsAsync(new List<Guid> { order.CourseId });
            //if (!courseResults.IsSuccessStatusCode)
            //{
            //    methodResult.AddError(courseResults.Error);
            //    return methodResult;
            //}
            //var course = courseResults.Content?.Result?.FirstOrDefault();

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order.Status = EnumOrderStatus.Payment;
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                //await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
                //{
                //    UserIds = new List<Guid>() { order.UserId },
                //    ObjectId = order.Id,
                //    ParamsMessage = new List<object> { course?.Name ?? string.Empty },
                //    Type = EnumNotificationType.Text,
                //    Content = EnumNotificationContent.OrderChangeStatus,
                //    PlatformCode = EnumPlatformCode.LMS
                //}, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
