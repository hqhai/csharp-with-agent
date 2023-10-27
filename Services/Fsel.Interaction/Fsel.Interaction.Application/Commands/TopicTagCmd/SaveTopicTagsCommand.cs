// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.TopicTagCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.TopicTags;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveTopicTagsCommand : SaveTopicTagsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveTopicTagsCommandHandler : IRequestHandler<SaveTopicTagsCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly ITopicTagRepository _topicTagRepository;

        public SaveTopicTagsCommandHandler(IMapper mapper, ITopicTagRepository topicTagRepository)
        {
            _mapper = mapper;
            _topicTagRepository = topicTagRepository;
        }

        public async Task<MethodResult<bool>> Handle(SaveTopicTagsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.TopicTags == null || request.TopicTags.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumeTopicTagErrorCode.TopicTagsCannotEmpty));
                return methodResult;
            }

            var topicIds = request.TopicTags.Select(x => x.Id).ToArray();
            var deleteTopics = await _topicTagRepository.Queryable.Where(x => !topicIds.Contains(x.Id)).ToListAsync(cancellationToken);

            foreach (var item in request.TopicTags)
            {
                TopicTag? topicTag;
                if (item.Id.HasValue)
                {
                    topicTag = await _topicTagRepository.GetByIdAsync(item.Id.Value);
                    if (topicTag == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(topicTag));
                        return methodResult;
                    }
                    topicTag = _mapper.Map(item, topicTag);
                }
                else
                {
                    topicTag = _mapper.Map<TopicTag>(item);
                }

                if (!topicTag.IsValid())
                {
                    methodResult.AddErrorBadRequest(topicTag.ErrorMessages);
                    return methodResult;
                }

                topicTag = item.Id.HasValue ? _topicTagRepository.Update(topicTag) : _topicTagRepository.Add(topicTag);
            }

            foreach (var item in deleteTopics)
            {
                await _topicTagRepository.DeleteAsync(item);
            }
            await _topicTagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
