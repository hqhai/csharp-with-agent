// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Events
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class JobActiveEventCommand : IRequest<MethodResult<bool>>
    {
    }

    public class JobActiveEventCommandHandler : IRequestHandler<JobActiveEventCommand, MethodResult<bool>>
    {
        private readonly IEventRepository _eventRepository;

        public JobActiveEventCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<bool>> Handle(JobActiveEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var currentDate = DateTime.UtcNow;
            var eventActive = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.Status == EnumEventPackageStatus.Active, cancellationToken);
            if (eventActive == null)
            {
                return methodResult;
            }
            var @event = await _eventRepository.Queryable.Where(p => p.StartDate.HasValue && p.EndDate.HasValue && p.StartDate.Value.Date <= currentDate.Date && p.EndDate.Value.Date >= currentDate.Date).FirstOrDefaultAsync(cancellationToken);
            if (@event == null)
            {
                @event = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
            }
            if (@event == null)
            {
                return methodResult;
            }
            if (eventActive.Id == @event.Id)
            {
                return methodResult;
            }
            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                @event.Status = EnumEventPackageStatus.Active;
                eventActive.Status = EnumEventPackageStatus.Inactive;
                @event = _eventRepository.Update(@event);
                eventActive = _eventRepository.Update(eventActive);
                await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
