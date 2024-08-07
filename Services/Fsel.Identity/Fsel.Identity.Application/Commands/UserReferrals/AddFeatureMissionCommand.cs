// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserReferrals;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class AddFeatureMissionCommand : AddFeatureMissionCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class AddFeatureMissionCommandHandler : IRequestHandler<AddFeatureMissionCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly AppSetting _appSetting;

        public AddFeatureMissionCommandHandler(IUserReferralRepository userReferralRepository, AppSetting appSetting)
        {
            _userReferralRepository = userReferralRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(AddFeatureMissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var userReferral = await _userReferralRepository.Queryable.FirstOrDefaultAsync(p => p.ReceiverId == request.ReceiverId, cancellationToken);
            if (userReferral == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            int token = 0;
            if (request.FeatureUserReferral == EnumFeatureUserReferral.PT)
            {
                token = _appSetting.UserReferralConfig?.PT ?? 0;
            }
            else if (request.FeatureUserReferral == EnumFeatureUserReferral.DoneUnit1)
            {
                token = _appSetting.UserReferralConfig?.DoneUnit1 ?? 0;
            }
            else
            {
                token = request.Token ?? 0;
            }

            var featureMissions = userReferral.FeatureMissions;
            if (featureMissions == null || !featureMissions.Any(p => p.FeatureUserReferral == request.FeatureUserReferral))
            {
                userReferral.FeatureMissions?.Add(new UserReferralToken
                {
                    FeatureUserReferral = request.FeatureUserReferral,
                    Token = token,
                });
            }

            await _userReferralRepository.ExecuteTransactionAsync(async () =>
            {
                _userReferralRepository.Update(userReferral);
                await _userReferralRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
