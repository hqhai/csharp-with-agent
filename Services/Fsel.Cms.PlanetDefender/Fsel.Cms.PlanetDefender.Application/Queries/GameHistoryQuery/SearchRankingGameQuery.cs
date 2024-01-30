// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
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
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchRankingGameQueryHandler(IGameHistoryRepository gameHistoryRepository
                                          , IStudentGameInfoRepository studentGameInfoRepository
                                          , IUserService userService
                                          , AuthContext authContext)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _studentGameInfoRepository = studentGameInfoRepository;
            _userService = userService;
            _authContext = authContext;
        }
        public async Task<MethodResult<PagingItemsModel<RankingGameModel>>> Handle(SearchRankingGameQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<RankingGameModel>> methodResult = new();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            IOrderedQueryable<RankingGameModel>? query = default;

            if (EnumRanking.Season == request.EnumRanking)
            {
                query = _gameHistoryRepository.Queryable
                                              .Where(x => x.CreatedDate.Year == DateTime.UtcNow.Year)
                                              .GroupBy(x => x.StudentGameInfoId)
                                              .Select(group => new RankingGameModel
                                              {
                                                  TotalScore = group.Sum(x => x.Score),
                                                  NickName = group.Select(x => x.StudentGameInfo).Select(x => x.NickName).FirstOrDefault(),
                                                  TagName = group.Select(x => x.StudentGameInfo).Select(x => x.StudentTagName).Select(x => x.TagName).FirstOrDefault(),
                                                  UpdatedDate = group.Select(x => x.StudentGameInfo).Select(x => x.UpdatedDate).FirstOrDefault(),
                                                  IsStudent = group.Any(x => x.StudentGameInfo!.StudentId == studentId),
                                              }).OrderByDescending(x => x.TotalScore)
                                                .ThenByDescending(x => x.UpdatedDate);
            }

            else if (EnumRanking.Level == request.EnumRanking)
            {
                query = _studentGameInfoRepository.Queryable
                                                  .Select(p => new RankingGameModel
                                                  {
                                                      NickName = p.NickName,
                                                      TagName = p.StudentTagName!.TagName,
                                                      Level = p.Level,
                                                      IsStudent = p.StudentId == studentId,
                                                  }).OrderByDescending(x => x.Level);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                .ApplyPaging(request)
                .AsNoTracking()
                .ToListAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<RankingGameModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
