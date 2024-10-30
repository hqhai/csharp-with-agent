// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckBannerPriorityExistenceCommand : IRequest<MethodResult<BannerPriorityExistenceModel>>
    {
        public IList<EnumCourseLevel>? CourseLevels { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class CheckBannerPriorityExistenceCommandHandler : IRequestHandler<CheckBannerPriorityExistenceCommand, MethodResult<BannerPriorityExistenceModel>>
    {
        private readonly IBannerScopeRepository _bannerScopeRepository;

        public CheckBannerPriorityExistenceCommandHandler(IBannerScopeRepository bannerScopeRepository)
        {
            _bannerScopeRepository = bannerScopeRepository;
        }

        public async Task<MethodResult<BannerPriorityExistenceModel>> Handle(CheckBannerPriorityExistenceCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<BannerPriorityExistenceModel> methodResult = new MethodResult<BannerPriorityExistenceModel>();

            var bannerScopes = await _bannerScopeRepository.Queryable
                                                           .Include(x => x.Banner)
                                                           .Where(x => x.Banner != null && request.StartDate <= x.Banner.EndDate && request.EndDate >= x.Banner.StartDate && x.IsPriority)
                                                           .Where(x => request.CourseLevels != null ? request.CourseLevels.Contains(x.CourseLevel) : (x.Banner!.Type == EnumBannerType.Warning))
                                                           .ToListAsync(cancellationToken);

            if (bannerScopes == null)
            {
                return methodResult;
            }

            BannerPriorityExistenceModel bannerPriorityExistence = new BannerPriorityExistenceModel();
            bannerPriorityExistence.IsWarring = true;
            bannerPriorityExistence.BannerPriorityExistenceDetails = new List<BannerPriorityExistenceDetailModel>();

            foreach (var bannerScope in bannerScopes)
            {
                BannerPriorityExistenceDetailModel bannerPriorityExistenceDetail = new BannerPriorityExistenceDetailModel();
                bannerPriorityExistenceDetail.Code = bannerScope.Banner?.Code;
                bannerPriorityExistenceDetail.CourseLevel = bannerScope.CourseLevel;
                bannerPriorityExistenceDetail.Content = bannerScope.Banner?.Content;
                bannerPriorityExistenceDetail.StartDate = bannerScope.Banner?.StartDate;
                bannerPriorityExistenceDetail.EndDate = bannerScope.Banner?.EndDate;

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
    }
}
