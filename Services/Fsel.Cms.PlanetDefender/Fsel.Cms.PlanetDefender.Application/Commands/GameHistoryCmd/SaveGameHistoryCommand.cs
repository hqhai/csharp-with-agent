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

    public class SaveGameHistoryCommand : SaveGameHistoryCommandModel, IRequest<MethodResult<GameHistoryModel>>
    {
    }

    public class SaveGameHistoryCommandHandler : IRequestHandler<SaveGameHistoryCommand, MethodResult<GameHistoryModel>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;

        public SaveGameHistoryCommandHandler(IGameHistoryRepository gameHistoryRepository, IMapper mapper, AuthContext authContext, IUserService userService, IStudentGameInfoRepository studentGameInfoRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _studentGameInfoRepository = studentGameInfoRepository;
        }

        public async Task<MethodResult<GameHistoryModel>> Handle(SaveGameHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GameHistoryModel> methodResult = new MethodResult<GameHistoryModel>();

            var gameHistory = new GameHistory();

            if (request.Id.HasValue)
            {
                gameHistory = await _gameHistoryRepository.GetByIdAsync(request.Id ?? default);
                if (gameHistory == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                gameHistory = _mapper.Map(request, gameHistory);
            }
            else
            {
                gameHistory = _mapper.Map<GameHistory>(request);
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var studentId = studentResult.Content?.Result?.Id;

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Include(p => p.StudentSpaceShips).FirstOrDefaultAsync(x => x.StudentId == studentId, cancellationToken);

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentGameInfo));
                return methodResult;
            }

            var spaceShipId = studentGameInfo.StudentSpaceShips.FirstOrDefault(p => p.IsActive)?.SpaceShipId;
            if (spaceShipId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentGameInfo));
                return methodResult;
            }

            await _gameHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                gameHistory.StudentGameInfoId = studentGameInfo.Id;
                gameHistory.SpaceShipId = spaceShipId ?? default;
                if (gameHistory.Id == default)
                {
                    gameHistory = _gameHistoryRepository.Add(gameHistory);
                    await _gameHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    gameHistory = _gameHistoryRepository.Update(gameHistory);
                    await _gameHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<GameHistoryModel>(gameHistory);
                return methodResult;
            });

            return methodResult;
        }
    }
}
