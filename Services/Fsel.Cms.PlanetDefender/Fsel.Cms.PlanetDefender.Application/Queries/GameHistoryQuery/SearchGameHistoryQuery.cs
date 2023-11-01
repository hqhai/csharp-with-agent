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
    using MediatR;

    public class SearchGameHistoryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<GameHistoryModel>>>
    {
    }

    public class SearchGameHistoryQueryHandler : IRequestHandler<SearchGameHistoryQuery, MethodResult<PagingItemsModel<GameHistoryModel>>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchGameHistoryQueryHandler(IGameHistoryRepository gameHistoryRepository, AuthContext authContext, IUserService userService)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<GameHistoryModel>>> Handle(SearchGameHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;

            var gameHistory = _gameHistoryRepository.Queryable.Where(x => x.StudentId == studentId);
            return await _gameHistoryRepository.GetListByPageResultAsync<GameHistoryModel>(gameHistory, request, cancellationToken).ConfigureAwait(false);
        }
    }
}
