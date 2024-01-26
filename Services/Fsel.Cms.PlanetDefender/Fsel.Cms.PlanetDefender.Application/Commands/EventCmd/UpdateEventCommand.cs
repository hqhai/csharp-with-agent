// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.EventCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.NewsAndUpdates;
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
        private readonly IEventRepository _newsAndUpdateRepository;

        public UpdateEventCommandHandler(IMapper mapper, IEventRepository newsAndUpdateRepository)
        {
            _mapper = mapper;
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<EventModel>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EventModel>();
            var newsAndUpdate = await _newsAndUpdateRepository.GetByIdAsync(request.Id);

            #region Validation

            if (newsAndUpdate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(newsAndUpdate));
                return methodResult;
            }

            _mapper.Map(request, newsAndUpdate);

            #endregion Validation

            await _newsAndUpdateRepository.ExecuteTransactionAsync(async () =>
            {
                newsAndUpdate = _newsAndUpdateRepository.Update(newsAndUpdate);

                await _newsAndUpdateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<EventModel>(newsAndUpdate);
                return methodResult;
            });

            return methodResult;
        }
    }
}
