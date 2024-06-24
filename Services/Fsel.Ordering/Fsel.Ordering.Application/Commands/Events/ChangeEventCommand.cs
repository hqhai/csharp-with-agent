// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
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
        }
    }
}
