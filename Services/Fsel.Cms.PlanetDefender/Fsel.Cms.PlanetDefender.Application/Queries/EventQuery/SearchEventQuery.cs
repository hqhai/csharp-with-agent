// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.NewsAndUpdateQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.NewsAndUpdates;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using static log4net.Appender.RollingFileAppender;

    public class SearchEventQuery : SearchEventQueryModel, IRequest<MethodResult<PagingItemsModel<EventModel>>>
    {
    }
    public class SearchNewsAndUpdateQueryHandler : IRequestHandler<SearchEventQuery, MethodResult<PagingItemsModel<EventModel>>>
    {
        private readonly IEventRepository _newsAndUpdateRepository;

        public SearchNewsAndUpdateQueryHandler(IEventRepository newsAndUpdateRepository)
        {
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<PagingItemsModel<EventModel>>> Handle(SearchEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var query = _newsAndUpdateRepository.Queryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Title ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.Status != null && request.Status == true)
            {
                query = query.Where(m => m.StartDate >= DateTime.UtcNow && DateTime.UtcNow <= DateTime.UtcNow.AddDays(1));
            }
            if (request.Status != null && request.Status == false)
            {
                query = query.Where(m => m.StartDate <= DateTime.UtcNow && DateTime.UtcNow >= DateTime.UtcNow.AddDays(1));
            }

            if (request.Type != null)
            {
                query = query.Where(m => m.Type == request.Type);
            }

            var methodResult = await _newsAndUpdateRepository.GetListByPageResultAsync<EventModel>(query, request, cancellationToken);
            return methodResult;
        }
    }
}
