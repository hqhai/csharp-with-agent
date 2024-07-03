// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Events
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchEventQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<EventModel>>>
    {
    }

    public class SearchEventQueryHandler : IRequestHandler<SearchEventQuery, MethodResult<PagingItemsModel<EventModel>>>
    {
        private readonly IEventRepository _eventRepository;

        public SearchEventQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<PagingItemsModel<EventModel>>> Handle(SearchEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<EventModel>>();

            var query = _eventRepository.Queryable.OrderByDescending(p => p.IsDefault).ThenBy(p => p.Status).Select(x => new EventModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                IsDefault = x.IsDefault,
                CreatedDate = x.CreatedDate,
                EventStatus = x.Status == EnumEventPackageStatus.Active,
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword) || !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplyPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<EventModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
