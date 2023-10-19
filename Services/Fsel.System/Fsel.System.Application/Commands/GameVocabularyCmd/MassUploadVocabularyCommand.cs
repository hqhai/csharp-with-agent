// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class MassUploadVocabularyCommand : MassUploadVocabularyCommandModel, IRequest<MethodResult<IList<GameVocabularyModel>>>
    {
    }

    public class MassUploadVocabularyCommandHandler : IRequestHandler<MassUploadVocabularyCommand, MethodResult<IList<GameVocabularyModel>>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public MassUploadVocabularyCommandHandler(IGameVocabularyRepository gameVocabularyRepository, IGameTopicRepository gameTopicRepository, IMapper mapper, IUserService userService)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<IList<GameVocabularyModel>>> Handle(MassUploadVocabularyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<GameVocabularyModel>> methodResult = new MethodResult<IList<GameVocabularyModel>>();

            if (request.GameVocabularies == null || request.GameVocabularies.Count == 0)
            {
                return methodResult;
            }

            #region Validate

            var gameTopicIds = request.GameVocabularies.Where(x => x.WordCategoryId.HasValue).Select(n => n.WordCategoryId).ToList();
            var gameTopics = await _gameTopicRepository.Queryable.Where(p => gameTopicIds.Contains(p.Id)).ToListAsync(cancellationToken);
            if (gameTopics.Count != gameTopicIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameTopicNotExist));
                return methodResult;
            }

            var codes = request.GameVocabularies.Where(x => !string.IsNullOrEmpty(x.Code)).Select(n => n.Code).ToList();
            if (codes.Any(p => p!.Length != 7 || !int.TryParse(p, out int checkCode)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.InvalidCode));
                return methodResult;
            }
            if (codes != null && codes.Count >= 2 && CheckDuplicate(codes))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.DuplicateCodes));
                return methodResult;
            }
            if (codes != null && codes.Count > 0 && _gameVocabularyRepository.Queryable.Any(p => codes.Contains(p.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.CodeAlreadyExist));
                return methodResult;
            }

            var keys = request.GameVocabularies.Where(x => !string.IsNullOrEmpty(x.Key)).Select(n => n.Key).ToList();
            if (keys != null && keys.Count >= 2 && CheckDuplicate(keys))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.DuplicateKeys));
                return methodResult;
            }
            if (keys != null && keys.Count > 0 && _gameVocabularyRepository.Queryable.Any(p => keys.Contains(p.Key)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.KeyAlreadyExist));
                return methodResult;
            }
            var countGameVocabulary = await _gameVocabularyRepository.Queryable.CountAsync(cancellationToken);

            #endregion Validate

            #region Get all Platform

            var platformsResult = await _userService.GetAllPlatform();
            var platforms = platformsResult.Content?.Result;

            #endregion Get all Platform

            await _gameVocabularyRepository.ExecuteTransactionAsync(async () =>
            {
                var gameVocabularies = _mapper.Map<IList<GameVocabulary>>(request.GameVocabularies);
                foreach (var item in gameVocabularies)
                {
                    if (string.IsNullOrEmpty(item.Code))
                    {
                        while (true)
                        {
                            string generateCode = countGameVocabulary.ToString("D7", CultureInfo.CurrentCulture);
                            if (!_gameVocabularyRepository.Queryable.Any(p => p.Code == generateCode) && !gameVocabularies.Any(p => p.Code == generateCode))
                            {
                                item.Code = generateCode;
                                break;
                            }
                            countGameVocabulary++;
                        }
                    }
                    if (!item.IsValid())
                    {
                        methodResult.AddError(item.ErrorMessages);
                        return methodResult;
                    }

                    #region Generate automatic PlatformIds

                    var listEnumGameVocabType = request.GameVocabularies.FirstOrDefault(p => p.Key == item.Key)?.GameVocabularyTypes?.Select(p => p.GameVocabType).ToList();
                    var platformIds = new List<Guid>();
                    if (listEnumGameVocabType?.Count > 0)
                    {
                        var platformCodes = PlatformCodeHelper.GetEnumPlatformCodes(listEnumGameVocabType);
                        platformIds = platforms?.Where(p => platformCodes != null && platformCodes.Contains(p.Code)).Select(p => p.Id).Distinct().ToList();
                    }

                    #endregion Generate automatic PlatformIds

                    #region Create GameVocabularyPlatforms

                    if (platformIds != null && platformIds.Count > 0)
                    {
                        platformIds.ForEach(p => { item.GameVocabularyPlatforms.Add(new GameVocabularyPlatform { PlatformId = p }); });
                    }

                    #endregion Create GameVocabularyPlatforms
                }

                await _gameVocabularyRepository.AddList(gameVocabularies);
                await _gameVocabularyRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<GameVocabularyModel>>(gameVocabularies);
                return methodResult;
            });

            return methodResult;
        }

        private static bool CheckDuplicate(IList<string?> objects)
        {
            var duplicate = objects.GroupBy(c => c).Where(p => p.Count() > 1).Select(x => x.Key);
            if (duplicate.Any())
            {
                return true;
            }
            return false;
        }
    }
}
