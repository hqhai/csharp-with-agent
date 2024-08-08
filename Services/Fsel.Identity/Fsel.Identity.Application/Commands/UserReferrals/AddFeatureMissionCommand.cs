// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserReferrals;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;

        public AddFeatureMissionCommandHandler(IUserReferralRepository userReferralRepository, AppSetting appSetting, CreateTokenHistoryPublisher createTokenHistoryPublisher)
        {
            _userReferralRepository = userReferralRepository;
            _appSetting = appSetting;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
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
            else if (request.FeatureUserReferral == EnumFeatureUserReferral.Payment)
            {
                token = request.Token ?? 0;
            }

            bool isAddDoneUnit1 = false;
            var featureMissions = userReferral.FeatureMissions;
            if (featureMissions == null || !featureMissions.Any(p => p.FeatureUserReferral == request.FeatureUserReferral))
            {
                if (featureMissions == null)
                {
                    featureMissions = new List<UserReferralToken>()
                    {
                        new UserReferralToken
                            {
                                FeatureUserReferral = request.FeatureUserReferral,
                                Token = token,
                            }
                    };
                }
                else
                {
                    featureMissions.Add(new UserReferralToken
                    {
                        FeatureUserReferral = request.FeatureUserReferral,
                        Token = token,
                    });
                }

                if (request.FeatureUserReferral == EnumFeatureUserReferral.Payment && !featureMissions.Any(p => p.FeatureUserReferral == EnumFeatureUserReferral.DoneUnit1))
                {
                    featureMissions.Add(new UserReferralToken
                    {
                        FeatureUserReferral = EnumFeatureUserReferral.DoneUnit1,
                        Token = token,
                    });
                    isAddDoneUnit1 = true;
                }

                userReferral.FeatureMissions = featureMissions;
            }
            else
            {
                return methodResult;
            }

            await _userReferralRepository.ExecuteTransactionAsync(async () =>
            {
                _userReferralRepository.Update(userReferral);
                await _userReferralRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var tokenMission = EnumTokenMission.FriendCompletePT;

                if (request.FeatureUserReferral == EnumFeatureUserReferral.PT)
                {
                    tokenMission = EnumTokenMission.FriendCompletePT;
                }
                else if (request.FeatureUserReferral == EnumFeatureUserReferral.DoneUnit1)
                {
                    tokenMission = EnumTokenMission.FriendCompleteUnit1;
                }
                else if (request.FeatureUserReferral == EnumFeatureUserReferral.Payment)
                {
                    tokenMission = EnumTokenMission.FriendCompletePayment;
                }

                await _createTokenHistoryPublisher.Publish(
                    new List<TokenHistoryQueueModel>
                        {
                            new TokenHistoryQueueModel
                                {
                                    VolatileToken = token,
                                    Type = EnumTokenHistoryType.Recevived,
                                    Feature = EnumTokenFeature.FriendMission,
                                    Mission = tokenMission,
                                    UserId = userReferral.SenderId,
                                }
                        },
                cancellationToken).ConfigureAwait(false);
                if (isAddDoneUnit1)
                {
                    await _createTokenHistoryPublisher.Publish(
                    new List<TokenHistoryQueueModel>
                        {
                            new TokenHistoryQueueModel
                                {
                                    VolatileToken = _appSetting.UserReferralConfig?.DoneUnit1 ?? 0,
                                    Type = EnumTokenHistoryType.Recevived,
                                    Feature = EnumTokenFeature.FriendMission,
                                    Mission = EnumTokenMission.FriendCompleteUnit1,
                                    UserId = userReferral.SenderId,
                                }
                        },
                cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
