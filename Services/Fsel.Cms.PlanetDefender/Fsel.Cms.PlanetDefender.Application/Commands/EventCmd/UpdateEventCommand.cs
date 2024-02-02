// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.EventCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.Events;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateEventCommand : UpdateEventCommandModel, IRequest<MethodResult<EventModel>>
    {
    }

    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, MethodResult<EventModel>>
    {
        private readonly IMapper _mapper;
        private readonly IEventRepository _eventRepository;

        public UpdateEventCommandHandler(IMapper mapper, IEventRepository eventRepository)
        {
            _mapper = mapper;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<EventModel>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EventModel>();
            var @event = await _eventRepository.GetByIdAsync(request.Id);

            #region Validation

            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@event));
                return methodResult;
            }

            _mapper.Map(request, @event);

            #endregion Validation

            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                @event = _eventRepository.Update(@event);

                await _eventRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<EventModel>(@event);
                return methodResult;
            });

            return methodResult;
        }
    }
}
