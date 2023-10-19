// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateGameVocabularyCommand : CreateGameVocabularyCommandModel, IRequest<MethodResult<GameVocabularyModel>>
    {
    }

    public class CreateGameVocabularyCommandHandler : IRequestHandler<CreateGameVocabularyCommand, MethodResult<GameVocabularyModel>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CreateGameVocabularyCommandHandler(IGameVocabularyRepository gameVocabularyRepository, IGameTopicRepository gameTopicRepository, IMapper mapper, IUserService userService)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<GameVocabularyModel>> Handle(CreateGameVocabularyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GameVocabularyModel> methodResult = new MethodResult<GameVocabularyModel>();

            #region validate

            if (!string.IsNullOrEmpty(request.Key) && _gameVocabularyRepository.Queryable.Any(p => p.Key == request.Key))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.KeyAlreadyExist));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Code) && (request.Code.Length != 7 || !int.TryParse(request.Code, out int checkCode)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.InvalidCode));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Code) && _gameVocabularyRepository.Queryable.Any(p => p.Code == request.Code))
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

            #endregion Get all Platform

            #region Generate automatic PlatformIds

            var listEnumGameVocabType = request.GameVocabularyTypes?.Select(p => p.GameVocabType).ToList();
            var platformIds = new List<Guid>();
            if (listEnumGameVocabType?.Count > 0)
            {
                var platformCodes = PlatformCodeHelper.GetEnumPlatformCodes(listEnumGameVocabType);
                platformIds = platforms?.Where(p => platformCodes != null && platformCodes.Contains(p.Code)).Select(p => p.Id).Distinct().ToList();
            }

            #endregion Generate automatic PlatformIds

            await _gameVocabularyRepository.ExecuteTransactionAsync(async () =>
            {
                var gameVocabulary = _mapper.Map<GameVocabulary>(request);
                if (string.IsNullOrEmpty(gameVocabulary.Code))
                {
                    var countGameVocabulary = await _gameVocabularyRepository.Queryable.CountAsync(cancellationToken);
                    while (true)
                    {
                        gameVocabulary.Code = countGameVocabulary.ToString("D7", CultureInfo.CurrentCulture);
                        if (!_gameVocabularyRepository.Queryable.Any(p => p.Code == gameVocabulary.Code))
                        {
                            break;
                        }
                        countGameVocabulary++;
                    }
                }
                if (!gameVocabulary.IsValid())
                {
                    methodResult.AddError(gameVocabulary.ErrorMessages);
                    return methodResult;
                }

                #region Create GameVocabularyPlatforms

                if (platformIds != null || platformIds?.Count > 0)
                {
                    platformIds.ForEach(p => { gameVocabulary.GameVocabularyPlatforms.Add(new GameVocabularyPlatform { PlatformId = p }); });
                }

                #endregion Create GameVocabularyPlatforms

                gameVocabulary = _gameVocabularyRepository.Add(gameVocabulary);
                await _gameVocabularyRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<GameVocabularyModel>(gameVocabulary);
                return methodResult;
            });

            return methodResult;
        }
    }
}
