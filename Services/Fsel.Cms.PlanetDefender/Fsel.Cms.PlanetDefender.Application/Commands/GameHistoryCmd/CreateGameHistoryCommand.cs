// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameHistorys;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateGameHistoryCommand : CreateGameHistoryCommandModel, IRequest<MethodResult<GameHistoryModel>>
    {
    }

    public class CreateGameHistoryCommandHandler : IRequestHandler<CreateGameHistoryCommand, MethodResult<GameHistoryModel>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ISpaceShipRepository _spaceShipRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;

        public CreateGameHistoryCommandHandler(IGameHistoryRepository gameHistoryRepository, IMapper mapper, ISpaceShipRepository spaceShipRepository, AuthContext authContext, IUserService userService, IStudentGameInfoRepository studentGameInfoRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _mapper = mapper;
            _spaceShipRepository = spaceShipRepository;
            _authContext = authContext;
            _userService = userService;
            _studentGameInfoRepository = studentGameInfoRepository;
        }

        public async Task<MethodResult<GameHistoryModel>> Handle(CreateGameHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GameHistoryModel> methodResult = new MethodResult<GameHistoryModel>();

            GameHistory gameHistory = _mapper.Map<GameHistory>(request);

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Where(x => x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentGameInfo));
                return methodResult;
            }

            if (!await _spaceShipRepository.AnyAsync(request.SpaceShipId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SpaceShipId));
                return methodResult;
            }
            await _gameHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                gameHistory.StudentGameInfoId = studentGameInfo.Id;
                gameHistory = _gameHistoryRepository.Add(gameHistory);
                await _gameHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<GameHistoryModel>(gameHistory);
                return methodResult;
            });

            return methodResult;
        }
    }
}
