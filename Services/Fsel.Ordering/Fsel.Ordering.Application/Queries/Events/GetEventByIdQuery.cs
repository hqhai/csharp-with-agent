// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Events
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetEventByIdQuery : IRequest<MethodResult<EventModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, MethodResult<EventModel>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IPackageRepository _packageRepository;

        public GetEventByIdQueryHandler(IEventRepository eventRepository, IMapper mapper, IPackageRepository packageRepository)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<EventModel>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EventModel>();

            var @event = await _eventRepository.Queryable
    .Include(p => p.PackageEvents)
    .Include(p => p.Translations)
    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), EnumEventErrorCode.EventNotExist.GetDescription());
                return methodResult;
            }

            var packages = await _packageRepository.Queryable
                .ToListAsync(cancellationToken);

            var @eventModel = _mapper.Map<EventModel>(@event);

            if (@eventModel.PackageEvents?.Any() == true)
            {
                var packageDict = packages.ToDictionary(p => p.Id);

                foreach (var item in @eventModel.PackageEvents)
                {
                    var package = packageDict[item.PackageId];
                    if (package != null)
                    {
                        item.Month = package.MonthNumber;
                        item.PackageEventStatus = item.Status == EnumEventPackageStatus.Active;
                    }
                }

                var existingPackageIds = @eventModel.PackageEvents.Select(pe => pe.PackageId).ToHashSet();

                foreach (var package in packages)
                {
                    if (!existingPackageIds.Contains(package.Id))
                    {
                        @eventModel.PackageEvents.Add(new PackageEventModel
                        {
                            Price = package.Price,
                            PackageId = package.Id,
                            PriceMonth = package.PriceMonth,
                            DayBonus = 0,
                            MonthBonus = 0,
                            Month = package.MonthNumber,
                            Status = EnumEventPackageStatus.Inactive,
                            PackageEventStatus = false,
                        });
                    }
                }

                @eventModel.PackageEvents = @eventModel.PackageEvents
                    .OrderBy(pe => pe.Month)
                    .ToList();
            }

            methodResult.Result = @eventModel;
            return methodResult;
        }
    }
}
