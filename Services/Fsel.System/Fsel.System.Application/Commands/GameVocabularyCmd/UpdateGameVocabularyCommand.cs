// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateGameVocabularyCommand : UpdateGameVocabularyCommandModel, IRequest<MethodResult<GameVocabularyModel>>
    {
    }

    public class UpdateGameVocabularyCommandHandler : IRequestHandler<UpdateGameVocabularyCommand, MethodResult<GameVocabularyModel>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IGameVocabularyTypeRepository _gameVocabularyTypeRepository;
        private readonly IGameVocabularyPlatformRepository _gameVocabularyPlatformRepository;

        public UpdateGameVocabularyCommandHandler(IGameVocabularyRepository gameVocabularyRepository, IGameTopicRepository gameTopicRepository, IMapper mapper, IUserService userService, IGameVocabularyTypeRepository gameVocabularyTypeRepository, IGameVocabularyPlatformRepository gameVocabularyPlatformRepository)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
            _userService = userService;
            _gameVocabularyTypeRepository = gameVocabularyTypeRepository;
            _gameVocabularyPlatformRepository = gameVocabularyPlatformRepository;
        }

        public async Task<MethodResult<GameVocabularyModel>> Handle(UpdateGameVocabularyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GameVocabularyModel> methodResult = new MethodResult<GameVocabularyModel>();

            var gameVocabulary = await _gameVocabularyRepository.GetByIdAsync(request.Id);
            if (gameVocabulary == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameVocabularyNotExist));
                return methodResult;
            }

            #region validate

            if (!string.IsNullOrEmpty(request.Key) && _gameVocabularyRepository.Queryable.Any(p => p.Key == request.Key && p.Id != gameVocabulary.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.KeyAlreadyExist));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Code) && (request.Code.Length != 7 || !int.TryParse(request.Code, out int checkCode)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.InvalidCode));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Code) && _gameVocabularyRepository.Queryable.Any(p => p.Code == request.Code && p.Id != gameVocabulary.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.CodeAlreadyExist));
                return methodResult;
            }

            if (request.WordCategoryId.HasValue && !_gameTopicRepository.Queryable.Any(p => p.Id == request.WordCategoryId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameTopicNotExist));
                return methodResult;
            }

            #endregion validate

            #region Get all Platform

            var platformsResult = await _userService.GetPlatformsQueryAsync(new BaseQueryModel { });
            var platforms = platformsResult.Content?.Result;
            var platformPDId = platforms?.FirstOrDefault(p => p.Code == EnumPlatformCode.PlanetDefender)?.Id;

            #endregion Get all Platform

            #region Generate automatic PlatformIds

            var listEnumGameVocabType = request.GameVocabularyTypeModels?.Select(p => p.GameVocabType).ToList();
            IList<EnumPlatformCode>? platformCodes = new List<EnumPlatformCode>();
            var platformIds = new List<Guid>();
            if (listEnumGameVocabType?.Count > 0)
            {
                platformCodes = PlatformCodeHelper.GetEnumPlatformCodes(listEnumGameVocabType);
                platformIds = platforms?.Where(p => platformCodes != null && platformCodes.Contains(p.Code)).Select(p => p.Id).Distinct().ToList();
            }

            #endregion Generate automatic PlatformIds

            #region Get delete and create GameVocabPlatforms

            var gameVocabPlatforms = await _gameVocabularyPlatformRepository.Queryable.Where(p => p.GameVocabularyId == gameVocabulary.Id && p.PlatformId != platformPDId).ToListAsync(cancellationToken);

            var deleteGameVocabPlatforms = gameVocabPlatforms.Where(p => platformIds == null || !platformIds.Contains(p.Id)).ToList();
            var createGameVocabPlatforms = platformIds?.Where(p => p != platformPDId && !gameVocabPlatforms.Select(x => x.Id).Contains(p)).ToList();

            #endregion Get delete and create GameVocabPlatforms

            await _gameVocabularyRepository.ExecuteTransactionAsync(async () =>
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    var countGameVocabulary = await _gameVocabularyRepository.Queryable.CountAsync(cancellationToken);
                    while (true)
                    {
                        request.Code = countGameVocabulary.ToString("D7", CultureInfo.CurrentCulture);
                        if (!_gameVocabularyRepository.Queryable.Any(p => p.Code == request.Code && p.Id != gameVocabulary.Id))
                        {
                            break;
                        }
                        countGameVocabulary++;
                    }
                }

                #region Delete GameVocabularyTypes

                var gameVocabularyTypeIds = request.GameVocabularyTypeModels?.Where(x => x.Id.HasValue).Select(p => p.Id).ToList();
                var gameVocabularyTypes = await _gameVocabularyTypeRepository.Queryable.Where(p => p.GameVocabType != EnumGameVocabType.JumbledSpelling && p.GameVocabularyId == gameVocabulary.Id && (gameVocabularyTypeIds == null || !gameVocabularyTypeIds.Contains(p.Id))).ToListAsync(cancellationToken);
                await _gameVocabularyTypeRepository.DeleteListAsync(gameVocabularyTypes);

                #endregion Delete GameVocabularyTypes

                #region Delete and create GameVocabularyPlatforms

                await _gameVocabularyPlatformRepository.DeleteListAsync(deleteGameVocabPlatforms);
                if (platformIds != null || platformIds?.Count > 0)
                {
                    IList<GameVocabularyPlatform> newGameVocabularyPlatforms = new List<GameVocabularyPlatform>();
                    platformIds.ForEach(p => { newGameVocabularyPlatforms.Add(new GameVocabularyPlatform { PlatformId = p, GameVocabularyId = gameVocabulary.Id }); });
                    await _gameVocabularyPlatformRepository.AddList(newGameVocabularyPlatforms);
                }
                await _gameVocabularyPlatformRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                #endregion Delete and create GameVocabularyPlatforms

                #region Update and create GameVocabularyTypes

                var updateGameVocabularyTypes = new List<GameVocabularyType>();
                var createGameVocabularyTypes = new List<GameVocabularyType>();
                if (request.GameVocabularyTypeModels != null || request.GameVocabularyTypeModels?.Count > 0)
                {
                    foreach (var item in request.GameVocabularyTypeModels)
                    {
                        if (item.Id.HasValue)
                        {
                            var gameVocabularyType = await _gameVocabularyTypeRepository.GetByIdAsync(item.Id ?? default);
                            if (gameVocabularyType == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameVocabularyTypeNotExist));
                                return methodResult;
                            }
                            gameVocabularyType = _mapper.Map(item, gameVocabularyType);
                            updateGameVocabularyTypes.Add(gameVocabularyType);
                        }
                        else
                        {
                            createGameVocabularyTypes.Add(new GameVocabularyType { GameVocabType = item.GameVocabType, QuestionContent = item.QuestionContent, GameVocabularyId = gameVocabulary.Id });
                        }
                    }
                }
                _gameVocabularyTypeRepository.UpdateList(updateGameVocabularyTypes);
                await _gameVocabularyTypeRepository.AddList(createGameVocabularyTypes);

                #endregion Update and create GameVocabularyTypes

                await _gameVocabularyTypeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _mapper.Map(request, gameVocabulary);
                if (!gameVocabulary.IsValid())
                {
                    methodResult.AddError(gameVocabulary.ErrorMessages);
                    return methodResult;
                }
                gameVocabulary = _gameVocabularyRepository.Update(gameVocabulary);
                await _gameVocabularyRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<GameVocabularyModel>(gameVocabulary);
                return methodResult;
            });

            return methodResult;
        }
    }
}
