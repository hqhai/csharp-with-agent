// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.EventCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteEventCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, MethodResult<bool>>
    {
        private readonly IEventRepository _eventRepository;

        public DeleteEventCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            #region Validation

            var @event = await _eventRepository.GetByIdAsync(request.Id);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@event));
                return methodResult;
            }

            #endregion Validation

            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _eventRepository.DeleteAsync(@event);
                await _eventRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
