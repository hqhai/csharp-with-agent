// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTopTrendingsQuery : GetPostTrendingQueryModel, IRequest<MethodResult<List<TopicTagModel>>>
    {
    }

    public class GetTopTrendingsQueryHandler : IRequestHandler<GetTopTrendingsQuery, MethodResult<List<TopicTagModel>>>
    {
        private readonly ITopicTagRepository _topicTagRepository;

        public GetTopTrendingsQueryHandler(ITopicTagRepository topicTagRepository)
        {
            _topicTagRepository = topicTagRepository;
        }

        public async Task<MethodResult<List<TopicTagModel>>> Handle(GetTopTrendingsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<TopicTagModel>>();

            var topicPostQuery = await _topicTagRepository.Queryable
                                        .Include(post => post.PostTags.Where(y => !y.IsDeleted))
                                        .OrderByDescending(t => t.PostTags.Count)
                                        .Select(tag => new TopicTagModel
                                        {
                                            Id = tag.Id,
                                            Name = tag.Name,
                                            Color = tag.Color,
                                            PostCount = tag.PostTags.Count
                                        }).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = topicPostQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
