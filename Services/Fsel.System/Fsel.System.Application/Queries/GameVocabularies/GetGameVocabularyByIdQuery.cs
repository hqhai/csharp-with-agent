// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GameVocabularies
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetGameVocabularyByIdQuery : IRequest<MethodResult<GameVocabularyModel>>
    {
        public Guid Id { get; set; }
    }
    public class GetGameVocabularyByIdQueryHandler : IRequestHandler<GetGameVocabularyByIdQuery, MethodResult<GameVocabularyModel>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;
        private readonly IMapper _mapper;
        public GetGameVocabularyByIdQueryHandler(IGameVocabularyRepository gameVocabularyRepository, IMapper mapper)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<GameVocabularyModel>> Handle(GetGameVocabularyByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GameVocabularyModel> methodResult = new MethodResult<GameVocabularyModel>();

            var gameVocabulary = await _gameVocabularyRepository.GetIncludeByIdAsync(request.Id);
            if (gameVocabulary == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameVocabularyNotExist));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<GameVocabularyModel>(gameVocabulary);
            return methodResult;

        }
    }
}
