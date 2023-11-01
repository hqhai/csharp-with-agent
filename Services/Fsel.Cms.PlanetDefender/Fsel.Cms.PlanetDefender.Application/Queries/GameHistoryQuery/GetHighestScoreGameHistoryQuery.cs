// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHighestScoreGameHistoryQuery : IRequest<MethodResult<GameHistoryModel>>
    {
    }

    public class GetHighestScoreGameHistoryQueryHandler : IRequestHandler<GetHighestScoreGameHistoryQuery, MethodResult<GameHistoryModel>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetHighestScoreGameHistoryQueryHandler(IGameHistoryRepository gameHistoryRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<GameHistoryModel>> Handle(GetHighestScoreGameHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<GameHistoryModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;

            var gameHistory = await _gameHistoryRepository.Queryable.Where(x => x.StudentId == studentId).OrderByDescending(p => p.RoundNumber).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<GameHistoryModel>(gameHistory);
            return methodResult;
        }
    }
}
