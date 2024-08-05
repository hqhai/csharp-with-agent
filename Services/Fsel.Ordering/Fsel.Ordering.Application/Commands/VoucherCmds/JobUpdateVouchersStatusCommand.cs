// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class JobUpdateVouchersStatusCommand : IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class JobUpdateVouchersStatusCommandHandler : IRequestHandler<JobUpdateVouchersStatusCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IVoucherRepository _voucherRepository;

        public JobUpdateVouchersStatusCommandHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(JobUpdateVouchersStatusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var currentDate = DateTime.UtcNow;

            var activeVouchers = await _voucherRepository.Queryable.Where(p => !p.IsActive && p.StartDate.Date <= currentDate.Date && (!p.EndDate.HasValue || p.EndDate.Value.Date >= currentDate.Date)).ToListAsync(cancellationToken);

            var inActiveVouchers = await _voucherRepository.Queryable.Where(p => p.IsActive && p.EndDate.HasValue && p.EndDate.Value.Date < currentDate.Date).ToListAsync(cancellationToken);

            activeVouchers.ForEach(p => p.IsActive = true);
            inActiveVouchers.ForEach(p => p.IsActive = false);

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                _voucherRepository.UpdateList(activeVouchers);
                _voucherRepository.UpdateList(inActiveVouchers);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
