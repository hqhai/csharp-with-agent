// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchGameHistoryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<GameHistoryModel>>>
    {
    }

    public class SearchGameHistoryQueryHandler : IRequestHandler<SearchGameHistoryQuery, MethodResult<PagingItemsModel<GameHistoryModel>>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;

        public SearchGameHistoryQueryHandler(IGameHistoryRepository gameHistoryRepository, AuthContext authContext, IUserService userService, IStudentGameInfoRepository studentGameInfoRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _authContext = authContext;
            _userService = userService;
            _studentGameInfoRepository = studentGameInfoRepository;
        }

        public async Task<MethodResult<PagingItemsModel<GameHistoryModel>>> Handle(SearchGameHistoryQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<GameHistoryModel>> methodResult = new MethodResult<PagingItemsModel<GameHistoryModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Where(x => x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);

            var gameHistory = _gameHistoryRepository.Queryable
                    .Where(x => x.StudentGameInfoId == studentGameInfo!.Id)
                    .Include(x => x.SpaceShip)
                    .Select(x => new GameHistoryModel
                    {
                        Id = x.Id,
                        CreatedDate = x.CreatedDate,
                        CreatedFullName = x.CreatedFullName,
                        DestroyNumber = x.DestroyNumber,
                        CreatedUserId = x.CreatedUserId,
                        ImpactNumber = x.ImpactNumber,
                        NumberOfToken = x.NumberOfToken,
                        RoundNumber = x.RoundNumber,
                        Score = x.Score,
                        SpaceShipId = x.SpaceShipId,
                        SpaceShipCode = x.SpaceShip!.Code,
                        ComboNumber = x.ComboNumber,
                        ZPlanetNumber = x.ZPlanetNumber,
                    });

            int totalItem = await gameHistory.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await gameHistory
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<GameHistoryModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
