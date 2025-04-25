namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Commands.OrderCmds.V1i2;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderEventByStudentCommand : IRequest<MethodResult<bool>>
    {
        public DateTime ExpiredDate { get; set; }
        public CreateOrderForStudentsEventCommandModel? Student { get; set; }
    }

    public class CreateOrderEventByStudentCommandHandler : IRequestHandler<CreateOrderEventByStudentCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        public CreateOrderEventByStudentCommandHandler(IOrderRepository orderRepository,
                                                       IMediator mediator)
        {
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(CreateOrderEventByStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Student);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var checkOrderUser = await _orderRepository.Queryable
                                                       .AnyAsync(x => x.UserId == request.Student.UserId && x.Status == EnumOrderStatus.Payment && x.ExpireDate.HasValue && x.ExpireDate >= request.ExpiredDate && x.RevenueType == EnumPaymentRevenueType.Revenue, cancellationToken);
            if (checkOrderUser)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.CannotAddStudentAlreadyOrder), nameof(checkOrderUser));
                return methodResult;
            }

            var createOrder = await _mediator.Send(new CreateOrderForStudentsEventCommand
            {
                ExpiredDate = request.ExpiredDate,
                Students = new List<CreateOrderForStudentsEventCommandModel> { request.Student }
            }, cancellationToken);

            if (!createOrder.IsOK)
            {
                methodResult.AddErrorBadRequest(createOrder.ErrorMessages.ToList());
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
