// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.LuckyTickets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateLuckyTicketWinningCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public DateTime WinningDate { get; set; }
        public IList<string>? Tickets { get; set; }
    }

    public class UpdateLuckyTicketWinningCommandHandler : IRequestHandler<UpdateLuckyTicketWinningCommand, MethodResult<VoidMethodResult>>
    {
        private readonly ILuckyTicketRepository _luckyTicketRepository;

        public UpdateLuckyTicketWinningCommandHandler(ILuckyTicketRepository luckyTicketRepository)
        {
            _luckyTicketRepository = luckyTicketRepository;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(UpdateLuckyTicketWinningCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            if (request.Tickets == null || request.Tickets.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var luckyTickets = await _luckyTicketRepository.Queryable.Where(p => p.Status == EnumLuckyTicketStatus.NotWon && !p.WinningDate.HasValue && !string.IsNullOrEmpty(p.Ticket) && request.Tickets.Contains(p.Ticket)).ToListAsync(cancellationToken);

            if (luckyTickets.Count != request.Tickets.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _luckyTicketRepository.ExecuteTransactionAsync(async () =>
            {
                luckyTickets.ForEach(p =>
                {
                    p.Status = EnumLuckyTicketStatus.Won;
                    p.WinningDate = request.WinningDate;
                });

                _luckyTicketRepository.UpdateList(luckyTickets);
                await _luckyTicketRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }
    }
}
