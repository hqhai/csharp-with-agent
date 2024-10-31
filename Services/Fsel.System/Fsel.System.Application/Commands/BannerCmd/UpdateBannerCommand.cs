// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateBannerCommand : UpdateBannerCommandModel, IRequest<MethodResult<BannerModel>>
    {
    }

    public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly IBannerScopeRepository _bannerScopeRepository;

        public UpdateBannerCommandHandler(IBannerRepository bannerRepository, IMapper mapper, IBannerScopeRepository bannerScopeRepository)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _bannerScopeRepository = bannerScopeRepository;
        }

        public async Task<MethodResult<BannerModel>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BannerModel>();

            (bool isValid, string errorCode, string field, object? value) = await ValidateBanner(request);
            if (!isValid)
            {
                methodResult.AddErrorBadRequest(nameof(errorCode), nameof(field), value);
                return methodResult;
            }

            var banner = await _bannerRepository.Queryable
                                                .Include(x => x.BannerScopes)
                                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (banner == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(banner));
                return methodResult;
            }

            _mapper.Map(request, banner);
            if (!banner.IsValid())
            {
                methodResult.AddErrorBadRequest(banner.ErrorMessages);
                return methodResult;
            }

            banner.StartDate = banner.StartDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
            banner.EndDate = banner.EndDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);

            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                await BannerScopeHandler(request, banner, cancellationToken);

                _bannerRepository.Update(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<BannerModel>(banner);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<VoidMethodResult> BannerScopeHandler(UpdateBannerCommand request, Banner banner, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.BannerScopes != null)
            {
                request.BannerScopes.ForEach(bannerScopeRequest =>
                {
                    if (bannerScopeRequest.Id.HasValue)
                    {
                        var bannerScope = banner.BannerScopes.FirstOrDefault(x => x.Id == bannerScopeRequest.Id);
                        _mapper.Map(bannerScopeRequest, bannerScope);
                    }
                    else
                    {
                        var newBannerScope = new BannerScope();
                        _mapper.Map(bannerScopeRequest, newBannerScope);
                        banner.BannerScopes.Add(newBannerScope);
                    }
                });

                var bannerScopeIds = request.BannerScopes.Where(x => x.Id.HasValue).Select(x => x.Id).ToList();
                var deleteBannerScopes = banner.BannerScopes.Where(x => x.Id != Guid.Empty && !bannerScopeIds.Contains(x.Id)).ToList();
                if (deleteBannerScopes.Count > 0)
                {
                    await _bannerScopeRepository.DeleteListAsync(deleteBannerScopes);
                    await _bannerScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }

                if (request.Type == EnumBannerType.Warning)
                {
                    var contentBanner = await _bannerRepository.Queryable.FirstOrDefaultAsync(x => x.Id != request.Id && x.Type == EnumBannerType.Warning && request.StartDate.Date <= x.EndDate.Date && request.EndDate.Date >= x.StartDate.Date, cancellationToken);
                    if (contentBanner != null)
                    {
                        contentBanner.Status = false;
                        _bannerRepository.Update(contentBanner);
                    }
                }
                else
                {
                    var courseLevels = request.BannerScopes.Where(x => x.IsPriority).Select(x => x.CourseLevel).ToList();
                    var priorityBanners = await _bannerScopeRepository.Queryable
                                                                      .Include(x => x.Banner)
                                                                      .Where(x => x.BannerId != request.Id && x.Banner != null && request.StartDate <= x.Banner.EndDate && request.EndDate >= x.Banner.StartDate)
                                                                      .Where(x => courseLevels.Contains(x.CourseLevel) && x.IsPriority)
                                                                      .ToListAsync(cancellationToken);

                    if (priorityBanners != null && priorityBanners.Any())
                    {
                        foreach (var priorityBanner in priorityBanners)
                        {
                            priorityBanner.Banner!.Status = false;
                        }

                        _bannerScopeRepository.UpdateList(priorityBanners);
                        await _bannerScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return methodResult;
        }

        private async Task<(bool, string, string, object?)> ValidateBanner(UpdateBannerCommand request)
        {
            bool condition = true;
            string errorMessage = string.Empty;
            string field = string.Empty;
            object? value = default;

            if (request.StartDate >= request.EndDate)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.StartDateGreaterThanEndDate);
            }

            if (request.Type == EnumBannerType.Popup && string.IsNullOrEmpty(request.FilePath))
            {
                condition = false;
                errorMessage = nameof(EnumSystemErrorCode.DataNotExist);
                field = nameof(EnumBannerType.Popup);
                value = request.FilePath;
            }
            else if (request.Type == EnumBannerType.Warning && string.IsNullOrEmpty(request.Content))
            {
                condition = false;
                errorMessage = nameof(EnumSystemErrorCode.DataNotExist);
                field = nameof(EnumBannerType.Warning);
                value = request.Content;
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && (request.DisplayStartDate >= request.DisplayEndDate))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.DisplayStartDateGreaterThanDisplayEndDate);
                field = nameof(request.DisplayStartDate);
                value = request.DisplayStartDate;
            }

            if (request.DisplayStartTime.HasValue && request.DisplayEndTime.HasValue && request.DisplayStartTime >= request.DisplayEndTime)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.DisplayStartTimeGreaterThanDisplayEndTime);
                field = nameof(request.DisplayStartTime);
                value = request.DisplayStartTime;
            }

            if (request.BannerScopes.GroupBy(x => x.CourseLevel).Any(x => x.Count() > 1))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.EachBannerOnlyOneCourseLevel);
                field = nameof(request.BannerScopes);
                value = request.BannerScopes;
            }

            if (request.BannerScopes.Any(x => x.ApplicableUserGroups != null && x.ApplicableUserGroups.Any(c => c == EnumApplicableUserGroup.Event) && x.CompetitionEventIds == null))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.CompetitionEventIdsRequired);
                field = nameof(request.BannerScopes);
                value = request.BannerScopes;
            }

            return (condition, errorMessage, field, value);
        }
    }
}
