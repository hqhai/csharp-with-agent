// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetRankingByYearQuery : IRequest<MethodResult<List<GameRankingModel>>>
    {
        public EnumRanking EnumRanking { get; set; }
    }

    public class GetRankingByYearQueryHandler : IRequestHandler<GetRankingByYearQuery, MethodResult<List<GameRankingModel>>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IStudentTagNameRepository _studentTagNameRepository;

        public GetRankingByYearQueryHandler(IGameHistoryRepository gameHistoryRepository
                                          , IStudentGameInfoRepository studentGameInfoRepository
                                          , IStudentTagNameRepository studentTagNameRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _studentGameInfoRepository = studentGameInfoRepository;
            _studentTagNameRepository = studentTagNameRepository;
        }
        public async Task<MethodResult<List<GameRankingModel>>> Handle(GetRankingByYearQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<GameRankingModel>> methodResult = new();

            var lstGameHistorys = await _gameHistoryRepository.Queryable
                                                             .Include(x => x.StudentGameInfo)
                                                             .ToListAsync(cancellationToken);

            if (lstGameHistorys.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            List<GameRankingModel> lstRankingModel = new();

            if (EnumRanking.Season == request.EnumRanking)
            {
                var query = lstGameHistorys.Where(x => x.CreatedDate.Year == DateTime.UtcNow.Year)
                                              .GroupBy(x => x.StudentGameInfoId)
                                              .Select(group => new GameRankingModel
                                              {
                                                  StudentGameInfoId = group.Key,
                                                  TotalScore = group.Sum(x => x.Score),
                                              }).OrderByDescending(x => x.TotalScore)
                                                .Take(100)
                                                .ToList();

                foreach (var item in query)
                {
                    var studentInfo = await _studentGameInfoRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.StudentGameInfoId, cancellationToken);
                    var tagName = await _studentTagNameRepository.Queryable.FirstOrDefaultAsync(x => x.Id == studentInfo!.TagNameId, cancellationToken);

                    GameRankingModel gameRankingModel = new GameRankingModel();

                    gameRankingModel.TotalScore = item.TotalScore;
                    gameRankingModel.NickName = studentInfo?.NickName ?? string.Empty;
                    gameRankingModel.TagName = tagName?.TagName ?? string.Empty;

                    lstRankingModel.Add(gameRankingModel);
                }

                methodResult.Result = lstRankingModel;
                return methodResult;
            }

            if (EnumRanking.Level == request.EnumRanking)
            {
                var query = lstGameHistorys.OrderByDescending(x => x.StudentGameInfo!.Level).Take(100).ToList();

                foreach (var item in query)
                {
                    var tagName = await _studentTagNameRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.StudentGameInfo.TagNameId, cancellationToken);

                    GameRankingModel gameRankingModel = new GameRankingModel();

                    gameRankingModel.NickName = item.StudentGameInfo!.NickName ?? string.Empty;
                    gameRankingModel.TagName = tagName?.TagName ?? string.Empty;
                    gameRankingModel.Level = item.StudentGameInfo.Level;

                    lstRankingModel.Add(gameRankingModel);
                }

                methodResult.Result = lstRankingModel;
                return methodResult;
            }

            return methodResult;
        }
    }
}
