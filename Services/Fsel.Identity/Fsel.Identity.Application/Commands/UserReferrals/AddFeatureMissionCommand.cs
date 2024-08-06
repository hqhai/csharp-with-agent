// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserReferrals;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
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

            var featureMissions = userReferral.FeatureMissions;

            if (featureMissions != null && )
            {
            }
        }
    }
}
