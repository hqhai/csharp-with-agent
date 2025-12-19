// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TopicQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTopicQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<TopicModel>>>
    {
    }

    public class SearchTopicQueryHandler : IRequestHandler<SearchTopicQuery, MethodResult<PagingItemsModel<TopicModel>>>
    {
        private readonly ITopicRepository _topicRepository;

        public SearchTopicQueryHandler(ITopicRepository topicRepository)
        {
            _topicRepository = topicRepository;
        }

        public async Task<MethodResult<PagingItemsModel<TopicModel>>> Handle(SearchTopicQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<TopicModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = from i in _topicRepository.Queryable
                        select new TopicModel
                        {
                            Id = i.Id,
                            Code = i.Code,
                            CreatedDate = i.CreatedDate,
                            CreatedFullName = i.CreatedFullName,
                            CreatedUserId = i.CreatedUserId,
                            Name = i.Name,
                            UpdatedDate = i.UpdatedDate,
                            UpdatedUserId = i.UpdatedUserId,
                            UpdatedFullName = i.UpdatedFullName,
                            Usage = i.Usage,
                            CountHomeWork = i.HomeWorks.Count
                        };
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    query = query.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                }
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<TopicModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
