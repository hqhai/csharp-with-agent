// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.PackageQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Queries.Events;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPackagesQuery : IRequest<MethodResult<List<PackageModel>>>
    {
        public bool? IsDefault { get; set; } = true;
    }

    public class GetPackagesQueryHandler : IRequestHandler<GetPackagesQuery, MethodResult<List<PackageModel>>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPackagesQueryHandler(IPackageRepository packageRepository,
                                       IMapper mapper,
                                       IMediator mediator)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<List<PackageModel>>> Handle(GetPackagesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PackageModel>>();

            var packageModels = new List<PackageModel>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var eventResult = await _mediator.Send(new GetCurrentEventQuery { IsDefault = request.IsDefault }, cancellationToken).ConfigureAwait(false);
            var @event = eventResult.Result;

            if (@event == null || @event.PackageEvents == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@event));
                return methodResult;
            }

            var eventModel = _mapper.Map<EventModel>(@event);
            var packages = await _packageRepository.Queryable.ToListAsync(cancellationToken);

            @event.PackageEvents = @event.PackageEvents.Where(p => p.Status == EnumEventPackageStatus.Active).ToList();

            foreach (var item in @event.PackageEvents)
            {
                var package = packages.FirstOrDefault(p => p.Id == item.PackageId);
                if (package == null)
                {
                    continue;
                }
                var packageModel = _mapper.Map<PackageModel>(package);
                packageModel.EventId = eventModel.Id;
                packageModel.Price = item.Price;
                packageModel.PriceMonth = item.PriceMonth;
                packageModel.MonthBonus = item.MonthBonus;
                packageModel.DayBonus = item.DayBonus;
                packageModel.ImagePaths = eventModel.ImagePaths;
                packageModel.EventDescription = eventModel.Description;
                packageModel.Suggests = item.Suggests;
                packageModels.Add(packageModel);
            }

            methodResult.Result = packageModels.OrderBy(p => p.MonthNumber).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
