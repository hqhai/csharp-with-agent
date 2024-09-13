// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserReferralsByUserQuery : IRequest<MethodResult<UserReferralsModel>>
    {
    }

    public class GetUserReferralsByUserQueryHandler : IRequestHandler<GetUserReferralsByUserQuery, MethodResult<UserReferralsModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserReferralRepository _userReferralRepository;

        public GetUserReferralsByUserQueryHandler(AuthContext authContext, IUserReferralRepository userReferralRepository)
        {
            _authContext = authContext;
            _userReferralRepository = userReferralRepository;
        }

        public async Task<MethodResult<UserReferralsModel>> Handle(GetUserReferralsByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserReferralsModel>();

            var userReferralsModel = new UserReferralsModel();

            var userReferrals = await _userReferralRepository.Queryable.Include(p => p.Receiver).Where(p => p.SenderId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

            var mission = StringHelper.GetEnumNames<EnumFeatureUserReferral>();

            foreach (var item in mission)
            {
                int numberUser = userReferrals.Where(p => p.FeatureMissions != null && p.FeatureMissions.Any(p => p.FeatureUserReferral.ToString() == item)).Count();
                int totalToken = userReferrals.Where(p => p.FeatureMissions != null && p.FeatureMissions.Count > 0).SelectMany(p => p.FeatureMissions!).Where(p => p.FeatureUserReferral.ToString() == item).Sum(p => p.Token);

                var enumValue = (EnumFeatureUserReferral)Enum.Parse(typeof(EnumFeatureUserReferral), item);

                userReferralsModel.DashboardUserReferrals.Add(new DashboardUserReferralsModel()
                {
                    FeatureUserReferral = enumValue,
                    NumberUser = numberUser,
                    TotalToken = totalToken
                });
            }

            foreach (var receiver in userReferrals)
            {
                userReferralsModel.UserReferrals.Add(new UserReferralModel()
                {
                    ReceiverId = receiver.ReceiverId,
                    FullName = receiver.Receiver?.FullName,
                    AvatarPath = receiver.Receiver?.AvatarPath,
                    FeatureMissions = receiver.FeatureMissions
                });
            }

            methodResult.Result = userReferralsModel;
            return methodResult;
        }
    }
}
