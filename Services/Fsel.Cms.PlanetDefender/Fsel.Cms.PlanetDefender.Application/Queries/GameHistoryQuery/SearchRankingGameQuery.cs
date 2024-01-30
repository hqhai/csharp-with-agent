// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameHistorys;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchRankingGameQuery : SearchRankingGameQueryModel, IRequest<MethodResult<PagingItemsModel<RankingGameModel>>>
    {
    }

    public class SearchRankingGameQueryHandler : IRequestHandler<SearchRankingGameQuery, MethodResult<PagingItemsModel<RankingGameModel>>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IStudentTagNameRepository _studentTagNameRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchRankingGameQueryHandler(IGameHistoryRepository gameHistoryRepository
                                          , IStudentGameInfoRepository studentGameInfoRepository
                                          , IStudentTagNameRepository studentTagNameRepository
                                          , IUserService userService
                                          , AuthContext authContext)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _studentGameInfoRepository = studentGameInfoRepository;
            _studentTagNameRepository = studentTagNameRepository;
            _userService = userService;
            _authContext = authContext;
        }
        public async Task<MethodResult<PagingItemsModel<RankingGameModel>>> Handle(SearchRankingGameQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<RankingGameModel>> methodResult = new();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            List<RankingGameModel> lstRankingGame = new();

            if (EnumRanking.Season == request.EnumRanking)
            {
                var query = _gameHistoryRepository.Queryable
                                  .Include(x => x.StudentGameInfo)
                                  .Where(x => x.CreatedDate.Year == DateTime.UtcNow.Year)
                                                           .GroupBy(x => x.StudentGameInfoId)
                                                           .Select(group => new
                                                           {
                                                               TotalScore = group.Sum(x => x.Score),
                                                               NickName = group.FirstOrDefault()!.StudentGameInfo!.NickName,
                                                               TagNameId = group.FirstOrDefault()!.StudentGameInfo!.TagNameId,
                                                               UpdatedDate = group.FirstOrDefault()!.StudentGameInfo!.UpdatedDate,
                                                               IsStudent = group.Any(x => x.StudentGameInfo!.StudentId == studentId),
                                                           }).OrderByDescending(x => x.TotalScore)
                                                             .ThenByDescending(x => x.UpdatedDate);

                int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                var lists = await query.ApplySortAndPaging(request)
                                              .AsNoTracking()
                                              .ToListAsync(cancellationToken)
                                              .ConfigureAwait(false);

                foreach (var item in lists)
                {
                    RankingGameModel rankingGame = new();

                    rankingGame.NickName = item.NickName;
                    rankingGame.TagName = _studentTagNameRepository.Queryable.FirstOrDefault(x => x.Id == item.TagNameId).TagName ?? string.Empty;
                    rankingGame.TotalScore = item.TotalScore;
                    rankingGame.IsStudent = item.IsStudent;

                    lstRankingGame.Add(rankingGame);
                }

                methodResult.Result = new PagingItemsModel<RankingGameModel>(lstRankingGame, request, totalItem);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            if (EnumRanking.Level == request.EnumRanking)
            {
                var query = _studentGameInfoRepository.Queryable
                                                      .Include(x => x.StudentTagName)
                                                      .Select(p => new
                                                      {
                                                          NickName = p.NickName,
                                                          TagName = p.StudentTagName!.TagName,
                                                          Level = p.Level,
                                                          IsStudent = p.StudentId == studentId ? true : false,
                                                      }).OrderByDescending(x => x.Level);

                int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in lists)
                {
                    RankingGameModel rankingGame = new();

                    rankingGame.NickName = item.NickName;
                    rankingGame.TagName = item.TagName;
                    rankingGame.Level = item.Level;
                    rankingGame.IsStudent = item.IsStudent;

                    lstRankingGame.Add(rankingGame);
                }

                methodResult.Result = new PagingItemsModel<RankingGameModel>(lstRankingGame, request, totalItem);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            return methodResult;
        }
    }
}
