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
            var @event = await _eventRepository.Queryable.Where(p => p.StartDate.HasValue && p.EndDate.HasValue && p.StartDate.Value.Date <= currentDate.Date && p.EndDate.Value.Date <= currentDate.Date).FirstOrDefaultAsync(cancellationToken);
            if (@event == null)
            {
                @event = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
            }
            if (@event == null)
            {
                return methodResult;
            }
            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                @event.Status = EnumEventPackageStatus.Active;
                @event = _eventRepository.Update(@event);
                await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
