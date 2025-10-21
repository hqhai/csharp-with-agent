// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Campus
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteOrderOfStudentsCampusCommand : DeleteOrderOfStudentsCampusCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class DeleteOrderOfStudentsCampusCommandHandler : IRequestHandler<DeleteOrderOfStudentsCampusCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderOfStudentsCampusCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteOrderOfStudentsCampusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var orders = await _orderRepository.Queryable.WhereBulkContains(request.UserIds, p => p.UserId).ToListAsync(cancellationToken);
            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                await _orderRepository.BulkDeleteList(orders);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
