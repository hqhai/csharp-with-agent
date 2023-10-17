// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameplayRuleConfigCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameplayRuleConfigs;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveListGameplayRuleConfigCommand : SaveListGameplayRuleConfigCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveListGameplayRuleConfigCommandHandler : IRequestHandler<SaveListGameplayRuleConfigCommand, MethodResult<bool>>
    {
        private readonly IGameplayRuleConfigRepository _gameplayRuleConfigRepository;
        private readonly IMapper _mapper;

        public SaveListGameplayRuleConfigCommandHandler(IGameplayRuleConfigRepository gameplayRuleConfigRepository, IMapper mapper)
        {
            _gameplayRuleConfigRepository = gameplayRuleConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(SaveListGameplayRuleConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.GameplayRuleConfigs == null || request.GameplayRuleConfigs.Count == 0)
            {
                return methodResult;
            }
            foreach (var item in request.GameplayRuleConfigs)
            {
                if (item.StartRoundNumber < 10 && (item.CurrentUnit + item.CurrentUnitOutside + item.PreviousUnit + item.PreviousUnitOutside) != 10)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.MaxLength));
                    return methodResult;
                }
                else if (item.StartRoundNumber >= 10 && (item.CurrentUnit + item.CurrentUnitOutside + item.PreviousUnit + item.PreviousUnitOutside) != 20)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.MaxLength));
                    return methodResult;
                }
            }
            var updateGameplayRuleConfigs = new List<GameplayRuleConfig>();
            var createGameplayRuleConfigs = new List<GameplayRuleConfig>();
            await _gameplayRuleConfigRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in request.GameplayRuleConfigs)
                {
                    if (item.Id.HasValue)
                    {
                        var gameplayRuleConfig = await _gameplayRuleConfigRepository.GetByIdAsync(item.Id ?? default);
                        if (gameplayRuleConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                            return methodResult;
                        }
                        _mapper.Map(item, gameplayRuleConfig);
                        updateGameplayRuleConfigs.Add(gameplayRuleConfig);
                    }
                    else
                    {
                        var newGameplayRuleConfig = _mapper.Map<GameplayRuleConfig>(item);
                        createGameplayRuleConfigs.Add(newGameplayRuleConfig);
                    }
                }
                _gameplayRuleConfigRepository.UpdateList(updateGameplayRuleConfigs);
                await _gameplayRuleConfigRepository.AddList(createGameplayRuleConfigs);
                await _gameplayRuleConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
