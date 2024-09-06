// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Events
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Events;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveEventCommand : SaveEventCommandModel, IRequest<MethodResult<EventModel>>
    {
    }

    public class SaveEventCommandHandler : IRequestHandler<SaveEventCommand, MethodResult<EventModel>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public SaveEventCommandHandler(IPackageRepository packageRepository, IEventRepository eventRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<EventModel>> Handle(SaveEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EventModel>();
            if (!request.Id.HasValue)
            {
                await Create(request, methodResult, cancellationToken);
            }
            else
            {
                await Update(request, methodResult, cancellationToken);
            }
            return methodResult;
        }

        private async Task Create(SaveEventCommandModel request, MethodResult<EventModel> methodResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(methodResult);

            if (request.PackageEvents == null || request.PackageEvents.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.MissingVersionOfPackage), EnumEventErrorCode.MissingVersionOfPackage.GetDescription());
                return;
            }
            if (request.Translations == null || request.Translations.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.MissingVersionOfTranslation), EnumEventErrorCode.MissingVersionOfTranslation.GetDescription());
                return;
            }
            if (await _eventRepository.Queryable.AnyAsync(p => p.Code.ToLower() == request.Code.ToLower(), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.CodeIsAlreadyExist), EnumEventErrorCode.CodeIsAlreadyExist.GetDescription());
                return;
            }
            var packageIds = request.PackageEvents.Select(x => x.PackageId).Distinct().ToList();
            if (packageIds == null || packageIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.PackageIdIsWrong), EnumEventErrorCode.PackageIdIsWrong.GetDescription());
                return;
            }
            if (await _packageRepository.Queryable.AnyAsync(p => !packageIds.Contains(p.Id) && p.Status == EnumPackageStatus.Active, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.PackageIdIsWrong), EnumEventErrorCode.PackageIdIsWrong.GetDescription());
                return;
            }
            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return;
            }
            if (request.StartDate.Value > request.EndDate.Value)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.StartDateIsGreaterThanEndDate), EnumEventErrorCode.StartDateIsGreaterThanEndDate.GetDescription());
                return;
            }

            var existEventDate = await _eventRepository.Queryable.AnyAsync(p => (p.StartDate <= request.StartDate && p.EndDate >= request.StartDate) || (p.StartDate <= request.EndDate && p.EndDate >= request.EndDate), cancellationToken);
            if (existEventDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.ThereWereEventsDuringThisTimePeriod), EnumEventErrorCode.ThereWereEventsDuringThisTimePeriod.GetDescription());
                return;
            }

            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                var @event = _mapper.Map<Event>(request);
                @event.Status = EnumEventPackageStatus.Inactive;
                if (!@event.IsValid())
                {
                    methodResult.AddError(@event.ErrorMessages);
                    return methodResult;
                }
                foreach (var item in @event.Translations)
                {
                    if (!item.IsValid())
                    {
                        methodResult.AddError(item.ErrorMessages);
                        return methodResult;
                    }
                }
                foreach (var item in @event.PackageEvents)
                {
                    if (!item.IsValid())
                    {
                        methodResult.AddError(item.ErrorMessages);
                        return methodResult;
                    }
                }
                @event = _eventRepository.Add(@event);
                await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<EventModel>(@event);
                return methodResult;
            });

            return;
        }

        private async Task Update(SaveEventCommandModel request, MethodResult<EventModel> methodResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(methodResult);
            ArgumentNullException.ThrowIfNull(request.Id);

            if (request.PackageEvents == null || request.PackageEvents.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.MissingVersionOfPackage), EnumEventErrorCode.MissingVersionOfPackage.GetDescription());
                return;
            }
            if (request.Translations == null || request.Translations.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.MissingVersionOfTranslation), EnumEventErrorCode.MissingVersionOfTranslation.GetDescription());
                return;
            }
            if (await _eventRepository.Queryable.AnyAsync(p => p.Code.ToLower() == request.Code.ToLower() && p.Id != request.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.CodeIsAlreadyExist), EnumEventErrorCode.CodeIsAlreadyExist.GetDescription());
                return;
            }
            var packageIds = request.PackageEvents.Select(x => x.PackageId).Distinct().ToList();
            if (packageIds == null || packageIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }
            if (await _packageRepository.Queryable.AnyAsync(p => !packageIds.Contains(p.Id), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var @event = await _eventRepository.Queryable.Include(p => p.Translations).Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            if (!@event.IsDefault)
            {
                if (!request.StartDate.HasValue || !request.EndDate.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return;
                }
                if (request.StartDate.Value.Date > request.EndDate.Value.Date)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.StartDateIsGreaterThanEndDate), EnumEventErrorCode.StartDateIsGreaterThanEndDate.GetDescription());
                    return;
                }
            }

            var existEventDate = await _eventRepository.Queryable.AnyAsync(p => p.Id != @event.Id && ((p.StartDate <= request.StartDate && p.EndDate >= request.StartDate) || (p.StartDate <= request.EndDate && p.EndDate >= request.EndDate)), cancellationToken);
            if (existEventDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.ThereWereEventsDuringThisTimePeriod), EnumEventErrorCode.ThereWereEventsDuringThisTimePeriod.GetDescription());
                return;
            }

            await _eventRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in @event.Translations)
                {
                    var translation = request.Translations.FirstOrDefault(x => x.Id == item.Id);
                    if (translation == null)
                    {
                        @event.Translations.Remove(item);
                    }
                    else
                    {
                        UpdateTranslation(translation, item);
                        if (!item.IsValid())
                        {
                            methodResult.AddError(item.ErrorMessages);
                            return methodResult;
                        }
                    }
                }
                foreach (var item in @event.PackageEvents)
                {
                    var packageEvent = request.PackageEvents.FirstOrDefault(x => x.Id == item.Id);
                    if (packageEvent == null)
                    {
                        var packageEventEntity = _mapper.Map<PackageEvent>(item);
                        if (!packageEventEntity.IsValid())
                        {
                            methodResult.AddError(packageEventEntity.ErrorMessages);
                            return methodResult;
                        }
                        @event.PackageEvents.Add(packageEventEntity);
                    }
                    else
                    {
                        UpdatePackageEvent(packageEvent, item);
                        if (!item.IsValid())
                        {
                            methodResult.AddError(item.ErrorMessages);
                            return methodResult;
                        }
                    }
                }
                UpdateEvent(request, @event);
                if (!@event.IsValid())
                {
                    methodResult.AddError(@event.ErrorMessages);
                    return methodResult;
                }
                @event = _eventRepository.Update(@event);
                await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<EventModel>(@event);
                return methodResult;
            });

            return;
        }

        private static void UpdateEvent(SaveEventCommandModel request, Event @event)
        {
            @event.Code = request.Code;
            @event.Name = request.Name;
            @event.StartDate = request.StartDate;
            @event.EndDate = request.EndDate;
            @event.Description = request.Description;
            @event.ImagePaths = request.ImagePaths;
        }

        private static void UpdateTranslation(SaveEventTranslationCommandModel request, EventTranslation eventTranslation)
        {
            eventTranslation.Language = request.Language;
            eventTranslation.Description = request.Description;
        }

        private static void UpdatePackageEvent(SavePackageEventCommandModel request, PackageEvent packageEvent)
        {
            packageEvent.Price = request.Price;
            packageEvent.PriceMonth = request.PriceMonth;
            packageEvent.DayBonus = request.DayBonus;
            packageEvent.MonthBonus = request.MonthBonus;
            packageEvent.Suggests = request.Suggests;
            packageEvent.Status = request.Status;
        }
    }
}
