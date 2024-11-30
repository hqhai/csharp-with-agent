// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBannerInDateQuery : IRequest<MethodResult<IList<BannerInDayModel>>>
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetBannerInDateQueryHandler : IRequestHandler<GetBannerInDateQuery, MethodResult<IList<BannerInDayModel>>>
    {
        private readonly IBannerRepository _bannerRepository;

        private const int AddOneDay = 1;

        public GetBannerInDateQueryHandler(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<MethodResult<IList<BannerInDayModel>>> Handle(GetBannerInDateQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<BannerInDayModel>> methodResult = new MethodResult<IList<BannerInDayModel>>();
            ArgumentNullException.ThrowIfNull(request);

            var bannes = await _bannerRepository.Queryable
                                                .Where(x => x.Status && request.StartDate.Date <= x.EndDate.Date && request.EndDate.Date >= x.StartDate.Date && x.BannerScopes.Any(c => c.CourseLevel == request.CourseLevel))
                                                .ToListAsync(cancellationToken);

            if (bannes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(bannes));
                return methodResult;
            }

            List<BannerInDayModel> bannerInDays = new List<BannerInDayModel>();
            for (DateTime date = request.StartDate; date <= request.EndDate; date = date.AddDays(AddOneDay))
            {
                var bannerDays = bannes.Where(x => x.StartDate.Date <= date.Date && x.EndDate.Date >= date.Date).ToList();

                BannerInDayModel bannerInDay = new BannerInDayModel
                {
                    Date = date.Date,
                    IsBanner = bannerDays.Any(x => x.Type == EnumBannerType.Popup),
                    IsHeading = bannerDays.Any(x => x.Type == EnumBannerType.Warning)
                };

                bannerInDays.Add(bannerInDay);
            }

            methodResult.Result = bannerInDays;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }

}
