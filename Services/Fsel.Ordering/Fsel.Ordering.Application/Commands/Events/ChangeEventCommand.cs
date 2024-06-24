// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChangeEventCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ChangeEventCommandHandler : IRequestHandler<ChangeEventCommand, MethodResult<bool>>
    {
        private readonly IEventRepository _eventRepository;

        public ChangeEventCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var @eventActive = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.Status == EnumEventPackageStatus.Active, cancellationToken);
            if (@eventActive == null || @eventActive.Id == request.Id)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }
            if (@eventActive.StartDate.HasValue && @eventActive.EndDate.HasValue && @eventActive.StartDate.Value.Date <= DateTime.UtcNow.Date && @eventActive.EndDate.Value.Date >= DateTime.UtcNow.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.TheEventIsActive), EnumEventErrorCode.TheEventIsActive.GetDescription());
                return methodResult;
            }
            var @event = await _eventRepository.GetByIdAsync(request.Id);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                @eventActive.Status = EnumEventPackageStatus.Inactive;
                @event.Status = EnumEventPackageStatus.Active;
                @event = _eventRepository.Update(@event);
                @eventActive = _eventRepository.Update(@eventActive);
                await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
