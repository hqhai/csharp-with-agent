namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DeleteOrderOfStudentsInEventCommand : DeleteOrderOfStudentsInEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class DeleteOrderOfStudentsInEventCommandHandler : IRequestHandler<DeleteOrderOfStudentsInEventCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderOfStudentsInEventCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteOrderOfStudentsInEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.UserIds == null || request.UserIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.UserIds));
                return methodResult;
            }

            var orders = await _orderRepository.Queryable.Include(p => p.OrderTransactions).WhereBulkContains(request.UserIds, p => p.UserId).ToListAsync(cancellationToken);

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                await _orderRepository.DeleteListAsync(orders);
                await _orderRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
