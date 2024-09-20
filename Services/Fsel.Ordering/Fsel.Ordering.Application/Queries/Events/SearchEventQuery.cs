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

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var query = await _eventRepository.Queryable.Select(x => new EventModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.StartDate.HasValue && x.EndDate.HasValue && x.StartDate <= currentDate && x.EndDate >= currentDate ? EnumEventPackageStatus.Active : EnumEventPackageStatus.Inactive,
                IsDefault = x.IsDefault,
                CreatedDate = x.CreatedDate,
                EventStatus = x.StartDate.HasValue && x.EndDate.HasValue && x.StartDate <= currentDate && x.EndDate >= currentDate
            }).ToListAsync(cancellationToken);

            if (!query.Any(p => p.EventStatus))
            {
                query.ForEach(p =>
                {
                    if (p.IsDefault)
                    {
                        p.EventStatus = true;
                        p.Status = EnumEventPackageStatus.Active;
                    }
                });
            }

            query = query.OrderByDescending(p => p.IsDefault).ThenByDescending(p => p.EventStatus = true).ThenByDescending(p => p.CreatedDate).ToList();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword) || !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword)).ToList();
            }

            int totalItem = query.Count;
            var lists = query
                    .ApplyPaging(request)
                    .ToList();

            methodResult.Result = new PagingItemsModel<EventModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
