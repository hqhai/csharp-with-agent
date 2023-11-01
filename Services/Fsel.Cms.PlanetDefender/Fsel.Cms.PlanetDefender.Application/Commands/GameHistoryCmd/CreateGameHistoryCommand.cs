// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameHistorys;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateGameHistoryCommand : CreateGameHistoryCommandModel, IRequest<MethodResult<GameHistoryModel>>
    {
    }
    public class CreateGameHistoryCommandHandler : IRequestHandler<CreateGameHistoryCommand, MethodResult<GameHistoryModel>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ISpaceShipRepository _spaceShipRepository;

        public CreateGameHistoryCommandHandler(IGameHistoryRepository gameHistoryRepository, IMapper mapper, ISpaceShipRepository spaceShipRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _mapper = mapper;
            _spaceShipRepository = spaceShipRepository;
        }

        public async Task<MethodResult<GameHistoryModel>> Handle(CreateGameHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GameHistoryModel> methodResult = new MethodResult<GameHistoryModel>();

            GameHistory gameHistory = _mapper.Map<GameHistory>(request);

            if (!await _spaceShipRepository.AnyAsync(request.SpaceShipId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SpaceShipId));
                return methodResult;
            }
            await _gameHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                gameHistory = _gameHistoryRepository.Add(gameHistory);
                await _gameHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<GameHistoryModel>(gameHistory);
                return methodResult;
            });

            return methodResult;
        }
    }
}
