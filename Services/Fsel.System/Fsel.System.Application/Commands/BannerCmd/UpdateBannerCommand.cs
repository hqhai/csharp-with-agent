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

            if (request.StartDate >= request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.StartDateGreaterThanEndDate));
                return methodResult;
            }

            if (request.Type == EnumBannerType.Popup && string.IsNullOrEmpty(request.FilePath))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumBannerType.Popup), request.FilePath);
                return methodResult;
            }
            else if (request.Type == EnumBannerType.Warning && string.IsNullOrEmpty(request.Content))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumBannerType.Warning), request.Content);
                return methodResult;
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && (!request.DisplayStartDate.HasValue || !request.DisplayEndDate.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.CustomFrequencyRequiresDisplayDate));
                return methodResult;
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && (request.DisplayStartDate >= request.DisplayEndDate))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.DisplayStartDateGreaterThanDisplayEndDate));
                return methodResult;
            }

            if (request.DisplayStartTime.HasValue && request.DisplayEndTime.HasValue && request.DisplayStartTime >= request.DisplayEndTime)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.DisplayStartTimeGreaterThanDisplayEndTime));
                return methodResult;
            }

            if (request.DisplayStartTime.HasValue && request.DisplayStartTime < 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.DisplayStartTimeMustGreaterThanZero));
                return methodResult;
            }

            if (request.BannerScopes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.BannerScopesRequired), nameof(request.BannerScopes), request.BannerScopes);
                return methodResult;
            }

            if (request.BannerScopes.GroupBy(x => x.CourseLevel).Any(x => x.Count() > 1))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.EachBannerOnlyOneCourseLevel), nameof(request.BannerScopes), request.BannerScopes);
                return methodResult;
            }

            if (request.BannerScopes.Any(x => x.TargetUsers == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.TargetUsersRequired));
                return methodResult;
            }

            if (request.BannerScopes.Any(x => x.ApplicableUserGroups == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.ApplicableUserGroupsRequired));
                return methodResult;
            }

            if (request.BannerScopes.Any(x => x.ApplicableUserGroups != null && x.ApplicableUserGroups.Any(c => c == EnumApplicableUserGroup.Event) && x.CompetitionEventIds == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.CompetitionEventIdsRequired));
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
                    var command = banner.BannerScopes.Where(x => x.Id != Guid.Empty && !bannerScopeIds.Contains(x.Id)).ToList();
                    if (command.Count > 0)
                    {
                        await _bannerScopeRepository.DeleteListAsync(command);
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

                _bannerRepository.Update(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<BannerModel>(banner);
                return methodResult;
            });

            return methodResult;
        }
    }
}
