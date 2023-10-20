// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameplayTimeConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Enums.ErrorCode;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameplayTimeConfigs;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveListGameplayTimeConfigCommand : SaveListGameplayTimeConfigCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveListGameplayTimeConfigCommandHandler : IRequestHandler<SaveListGameplayTimeConfigCommand, MethodResult<bool>>
    {
        private readonly IGameplayTimeConfigRepository _gameplayTimeConfigRepository;
        private readonly IMapper _mapper;

        public SaveListGameplayTimeConfigCommandHandler(IGameplayTimeConfigRepository gameplayTimeConfigRepository, IMapper mapper)
        {
            _gameplayTimeConfigRepository = gameplayTimeConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(SaveListGameplayTimeConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.GameplayTimeConfigs == null || request.GameplayTimeConfigs.Count == 0)
            {
                return methodResult;
            }
            int min = request.GameplayTimeConfigs.Select(p => p.RoundNumber).Min();
            int max = request.GameplayTimeConfigs.Select(p => p.RoundNumber).Max();

            for (var i = min; i <= max; i++)
            {
                var duplicateType = request.GameplayTimeConfigs.Where(p => p.RoundNumber == i).Select(m => m.GameVocabPDType).GroupBy(x => x).Where(n => n.Count() > 1);
                if (duplicateType.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                    return methodResult;
                }
                var totalItem = request.GameplayTimeConfigs.Where(p => p.RoundNumber == i);
                if (totalItem.Count() != 5)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                if (totalItem.Sum(x => x.Percent) != 100)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumGameplayTimeConfigErrorCode.TheTotalPercentageDoesNotEqual100));
                    return methodResult;
                }
            }
            var gameplayTimeConfigIds = request.GameplayTimeConfigs.Where(x => x.Id.HasValue).Select(n => n.Id).ToList();
            var deleteGameplayTimeConfigs = await _gameplayTimeConfigRepository.Queryable.Where(p => !gameplayTimeConfigIds.Contains(p.Id)).ToListAsync(cancellationToken);
            var updateGameplayTimeConfigs = new List<GameplayTimeConfig>();
            var createGameplayTimeConfigs = new List<GameplayTimeConfig>();
            await _gameplayTimeConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (deleteGameplayTimeConfigs.Count > 0)
                {
                    await _gameplayTimeConfigRepository.DeleteListAsync(deleteGameplayTimeConfigs);
                }
                foreach (var item in request.GameplayTimeConfigs)
                {
                    if (item.Id.HasValue)
                    {
                        var gameplayTimeConfig = await _gameplayTimeConfigRepository.GetByIdAsync(item.Id ?? default);
                        if (gameplayTimeConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumGameplayTimeConfigErrorCode.GameplayTimeConfigNotExist));
                            return methodResult;
                        }
                        _mapper.Map(item, gameplayTimeConfig);
                        updateGameplayTimeConfigs.Add(gameplayTimeConfig);
                    }
                    else
                    {
                        var newGameplayTimeConfig = _mapper.Map<GameplayTimeConfig>(item);
                        createGameplayTimeConfigs.Add(newGameplayTimeConfig);
                    }
                }
                _gameplayTimeConfigRepository.UpdateList(updateGameplayTimeConfigs);
                await _gameplayTimeConfigRepository.AddList(createGameplayTimeConfigs);
                await _gameplayTimeConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
