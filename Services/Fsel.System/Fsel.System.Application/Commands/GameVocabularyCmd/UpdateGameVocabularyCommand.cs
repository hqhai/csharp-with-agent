// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateGameVocabularyCommand : UpdateGameVocabularyCommandModel, IRequest<MethodResult<GameVocabularyModel>>
    {
    }

    public class UpdateGameVocabularyCommandHandler : IRequestHandler<UpdateGameVocabularyCommand, MethodResult<GameVocabularyModel>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;
        private readonly IGameCenterRepository _gameCenterRepository;
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;

        public UpdateGameVocabularyCommandHandler(IGameVocabularyRepository gameVocabularyRepository, IGameCenterRepository gameCenterRepository, IGameTopicRepository gameTopicRepository, IMapper mapper)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _gameCenterRepository = gameCenterRepository;
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
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

            if (request.GameCenterId.HasValue && !_gameCenterRepository.Queryable.Any(p => p.Id == request.GameCenterId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameCenterNotExist));
                return methodResult;
            }

            if (request.WordCategoryId.HasValue && !_gameTopicRepository.Queryable.Any(p => p.Id == request.WordCategoryId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameTopicNotExist));
                return methodResult;
            }
            #endregion validate

            await _gameVocabularyRepository.ExecuteTransactionAsync(async () =>
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    var countGameVocabulary = await _gameVocabularyRepository.Queryable.CountAsync(cancellationToken);
                    while (true)
                    {
                        countGameVocabulary++;
                        request.Code = countGameVocabulary.ToString("D7", CultureInfo.CurrentCulture);
                        if (!_gameVocabularyRepository.Queryable.Any(p => p.Code == request.Code))
                        {
                            break;
                        }
                        countGameVocabulary++;
                    }
                }
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
