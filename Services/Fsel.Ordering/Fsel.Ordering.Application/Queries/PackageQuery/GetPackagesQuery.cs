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
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPackagesQuery : IRequest<MethodResult<List<PackageModel>>>
    {
    }

    public class GetPackagesQueryHandler : IRequestHandler<GetPackagesQuery, MethodResult<List<PackageModel>>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;
        private readonly IEventRepository _eventRepository;

        public GetPackagesQueryHandler(IPackageRepository packageRepository, IMapper mapper, IEventRepository eventRepository)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<List<PackageModel>>> Handle(GetPackagesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PackageModel>>();

            var packageModels = new List<PackageModel>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.StartDate.HasValue && p.EndDate.HasValue && p.StartDate.Value <= currentDate && p.EndDate >= currentDate, cancellationToken);

            if (@event == null)
            {
                @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
            }

            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@event));
                return methodResult;
            }

            var eventModel = _mapper.Map<EventModel>(@event);
            var packages = await _packageRepository.Queryable.ToListAsync(cancellationToken);

            foreach (var item in @event.PackageEvents)
            {
                var package = packages.FirstOrDefault(p => p.Id == item.PackageId);
                if (package == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                    return methodResult;
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
