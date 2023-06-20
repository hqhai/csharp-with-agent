// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.TopicTagQuery
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTopicTagsQuery : IRequest<MethodResult<IList<TopicTagModel>>>
    {
    }

    public class GetTopicTagsQueryHandler : IRequestHandler<GetTopicTagsQuery, MethodResult<IList<TopicTagModel>>>
    {
        private readonly ITopicTagRepository _topicTagRepository;
        private readonly IMapper _mapper;

        public GetTopicTagsQueryHandler(ITopicTagRepository topicTagRepository, IMapper mapper)
        {
            _topicTagRepository = topicTagRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TopicTagModel>>> Handle(GetTopicTagsQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<TopicTagModel>>();

            var topicTags = await _topicTagRepository.Queryable.OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = _mapper.Map<IList<TopicTagModel>>(topicTags);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
