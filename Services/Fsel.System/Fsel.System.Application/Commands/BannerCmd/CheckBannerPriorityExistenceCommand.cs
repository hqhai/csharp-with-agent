// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.Models.QueryModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.BannerScopes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckBannerPriorityExistenceCommand : IRequest<MethodResult<BannerPriorityExistenceModel>>
    {
        public Guid? Id { get; set; }

        public EnumBannerType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IList<CreateBannerScopeCommandModel>? BannerScopes { get; set; }
    }

    public class CheckBannerPriorityExistenceCommandHandler : IRequestHandler<CheckBannerPriorityExistenceCommand, MethodResult<BannerPriorityExistenceModel>>
    {
        private readonly IBannerScopeRepository _bannerScopeRepository;
        private readonly IUserService _userService;

        public CheckBannerPriorityExistenceCommandHandler(IBannerScopeRepository bannerScopeRepository,
                                                          IUserService userService)
        {
            _bannerScopeRepository = bannerScopeRepository;
            _userService = userService;
        }

        public async Task<MethodResult<BannerPriorityExistenceModel>> Handle(CheckBannerPriorityExistenceCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<BannerPriorityExistenceModel> methodResult = new MethodResult<BannerPriorityExistenceModel>();

            var bannerScopeQuerys = await _bannerScopeRepository.Queryable
                                                                .Include(x => x.Banner)
                                                                .Where(x => x.Banner != null && request.StartDate <= x.Banner.EndDate && request.EndDate >= x.Banner.StartDate && x.Banner!.Type == request.Type && x.Banner.Status)
                                                                .ToListAsync(cancellationToken);

            var applicableUserDefaults = request.BannerScopes?.Where(x => x.ApplicableUser == EnumApplicableUserGroup.Default).ToList();
            var applicableUserEvents = request.BannerScopes?.Where(x => x.ApplicableUser == EnumApplicableUserGroup.Event).ToList();
            List<BannerScope> bannerScopes = new List<BannerScope>();

            if (request.Type == EnumBannerType.Popup)
            {
                if (applicableUserDefaults != null && applicableUserDefaults.Any())
                {
                    var bannerScopeDefaults = bannerScopeQuerys.Where(x => !x.CompetitionEventId.HasValue && x.IsPriority && applicableUserDefaults.Any(p => p.IsPriority && p.CourseLevel == x.CourseLevel)).ToList();
                    bannerScopes.AddRange(bannerScopeDefaults);
                }

                if (applicableUserEvents != null && applicableUserEvents.Any())
                {
                    var bannerScopeEvents = bannerScopeQuerys.Where(x => x.IsPriority && applicableUserEvents.Any(p => p.IsPriority && p.CourseLevel == x.CourseLevel && p.CompetitionEventId == x.CompetitionEventId)).ToList();
                    bannerScopes.AddRange(bannerScopeEvents);
                }
            }

            if (request.Type == EnumBannerType.Left || request.Type == EnumBannerType.Warning)
            {
                if (applicableUserDefaults != null && applicableUserDefaults.Any())
                {
                    var bannerScopeDefaults = bannerScopeQuerys.Where(x => !x.CompetitionEventId.HasValue && x.Banner!.Status && applicableUserDefaults.Any(p => p.CourseLevel == x.CourseLevel)).ToList();
                    bannerScopes.AddRange(bannerScopeDefaults);
                }

                if (applicableUserEvents != null && applicableUserEvents.Any())
                {
                    var bannerScopeEvents = bannerScopeQuerys.Where(x => x.Banner!.Status && applicableUserEvents.Any(p => p.CourseLevel == x.CourseLevel && p.CompetitionEventId == x.CompetitionEventId)).ToList();
                    bannerScopes.AddRange(bannerScopeEvents);
                }
            }

            if (request.Id.HasValue)
            {
                bannerScopes = bannerScopes.Where(x => x.Banner!.Id != request.Id).ToList();
            }

            if (!bannerScopes.Any())
            {
                return methodResult;
            }

            BannerPriorityExistenceModel bannerPriorityExistence = new BannerPriorityExistenceModel();
            bannerPriorityExistence.IsWarring = true;
            bannerPriorityExistence.BannerPriorityExistenceDetails = new List<BannerPriorityExistenceDetailModel>();

            var competitionEventIds = bannerScopes.Where(x => x.CompetitionEventId.HasValue).Select(x => x.CompetitionEventId!.Value).ToList();
            var eventResults = await _userService.GetEventByIds(new GetEventByIdsModel { Ids = competitionEventIds });
            var events = eventResults.Content?.Result;

            foreach (var bannerScope in bannerScopes.DistinctBy(x => x.Id))
            {
                BannerPriorityExistenceDetailModel bannerPriorityExistenceDetail = new BannerPriorityExistenceDetailModel();
                bannerPriorityExistenceDetail.Code = bannerScope.Banner?.Code;
                bannerPriorityExistenceDetail.CourseLevel = bannerScope.CourseLevel;
                bannerPriorityExistenceDetail.Content = bannerScope.Banner?.Content;
                bannerPriorityExistenceDetail.StartDate = bannerScope.Banner?.StartDate;
                bannerPriorityExistenceDetail.EndDate = bannerScope.Banner?.EndDate;
                bannerPriorityExistenceDetail.EventCode = events?.FirstOrDefault(x => x.Id == bannerScope.CompetitionEventId)?.EventCode;

                bannerPriorityExistence.BannerPriorityExistenceDetails.Add(bannerPriorityExistenceDetail);
            }

            methodResult.Result = bannerPriorityExistence;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }

    public class BannerPriorityExistenceModel
    {
        public bool IsWarring { get; set; }

        public IList<BannerPriorityExistenceDetailModel>? BannerPriorityExistenceDetails { get; set; }
    }

    public class BannerPriorityExistenceDetailModel
    {
        public string? Code { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }

        public string? Content { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? EventCode { get; set; }
    }
}
