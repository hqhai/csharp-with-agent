// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameTopicCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.GameTopics;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveListGameTopicCommand : SaveListGameTopicCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveListGameTopicCommandHandler : IRequestHandler<SaveListGameTopicCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IGameTopicRepository _gameTopicRepository;

        public SaveListGameTopicCommandHandler(IMapper mapper, IGameTopicRepository gameTopicRepository)
        {
            _mapper = mapper;
            _gameTopicRepository = gameTopicRepository;
        }

        public async Task<MethodResult<bool>> Handle(SaveListGameTopicCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.GameTopics == null || request.GameTopics.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var gameTopicIds = request.GameTopics.Select(x => x.Id).ToArray();
            var gameTopics = await _gameTopicRepository.GetByIdsAsync(request.GameTopics.Select(x => x.Id ?? default).ToList());

            foreach (var item in request.GameTopics)
            {
                GameTopic? gameTopic;
                if (item.Id.HasValue)
                {
                    gameTopic = gameTopics.FirstOrDefault(x => x.Id == item.Id.Value);
                    if (gameTopic == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(gameTopic));
                        return methodResult;
                    }
                    gameTopic = _mapper.Map(item, gameTopic);
                }
                else
                {
                    gameTopic = _mapper.Map<GameTopic>(item);
                }

                if (!gameTopic.IsValid())
                {
                    methodResult.AddErrorBadRequest(gameTopic.ErrorMessages);
                    return methodResult;
                }

                gameTopic = item.Id.HasValue ? _gameTopicRepository.Update(gameTopic) : _gameTopicRepository.Add(gameTopic);
            }

            await _gameTopicRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
