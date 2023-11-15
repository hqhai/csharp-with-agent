// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetGameHistoryQuery : IRequest<MethodResult<GameHistoryModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetGameHistoryQueryHandler : IRequestHandler<GetGameHistoryQuery, MethodResult<GameHistoryModel>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IGameAnswerRepository _gameAnswerRepository;

        public GetGameHistoryQueryHandler(IGameHistoryRepository gameHistoryRepository, ISystemService systemService, IMapper mapper, IGameAnswerRepository gameAnswerRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _systemService = systemService;
            _mapper = mapper;
            _gameAnswerRepository = gameAnswerRepository;
        }

        public async Task<MethodResult<GameHistoryModel>> Handle(GetGameHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<GameHistoryModel>();

            var gameHistory = await _gameHistoryRepository.Queryable.Include(p => p.GameAnswers).Include(n => n.SpaceShip).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (gameHistory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var gameHistoryModel = _mapper.Map<GameHistoryModel>(gameHistory);

            gameHistoryModel.SpaceShipCode = gameHistory.SpaceShip?.Code;
            gameHistoryModel.TotalScore = _gameHistoryRepository.Queryable.Where(p => p.StudentGameInfoId == gameHistoryModel.StudentGameInfoId).Sum(x => x.Score);

            var answerInCorrect = await _gameAnswerRepository.Queryable.Where(p => p.GameHistoryId == gameHistoryModel.Id && !p.IsCorrect).ToListAsync(cancellationToken);

            var gameVocabulariesResult = await _systemService.ExecuteListGameVocabularyQueryAsync(new BaseQueryModel { IncludePaths = new List<string> { "GameVocabularyTypes" } });
            if (!gameVocabulariesResult.IsSuccessStatusCode || gameVocabulariesResult.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var gameVocabularies = gameVocabulariesResult.Content.Result;

            gameHistoryModel.StudentAnswers = answerInCorrect.Select(p => new StudentAnswerModel
            {
                Type = gameVocabularies.FirstOrDefault(x => x.Id == p.GameVocabularyId)?.GameVocabularyTypes?.FirstOrDefault(n => n.Id == p.GameVocabularyTypeId)?.GameVocabType,
                QuestionContent = gameVocabularies.FirstOrDefault(x => x.Id == p.GameVocabularyId)?.GameVocabularyTypes?.FirstOrDefault(n => n.Id == p.GameVocabularyTypeId)?.QuestionContent,
                Answer = p.Answer,
                IsCorrect = p.IsCorrect,
                Key = gameVocabularies.FirstOrDefault(x => x.Id == p.GameVocabularyId)?.Key
            }).ToList();

            methodResult.Result = gameHistoryModel;
            return methodResult;
        }
    }
}
