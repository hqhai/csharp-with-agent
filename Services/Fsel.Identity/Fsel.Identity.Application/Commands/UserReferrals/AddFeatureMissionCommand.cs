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
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IStudentRepository _studentRepository;

        public AddFeatureMissionCommandHandler(IUserReferralRepository userReferralRepository, AppSetting appSetting, CreateTokenHistoryPublisher createTokenHistoryPublisher, NotificationMessagePublisher notificationMessagePublisher, IStudentRepository studentRepository)
        {
            _userReferralRepository = userReferralRepository;
            _appSetting = appSetting;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _studentRepository = studentRepository;
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
            if (userReferral.IsCoinRewarded)
            {
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
                                CreatedDate = DateTime.UtcNow,
                                PackageId = request.PackageId,
                            }
                    };
                }
                else
                {
                    featureMissions.Add(new UserReferralToken
                    {
                        FeatureUserReferral = request.FeatureUserReferral,
                        Token = token,
                        CreatedDate = DateTime.UtcNow,
                        PackageId = request.PackageId,
                    });
                }

                if (request.FeatureUserReferral == EnumFeatureUserReferral.Payment && !featureMissions.Any(p => p.FeatureUserReferral == EnumFeatureUserReferral.DoneUnit1))
                {
                    featureMissions.Add(new UserReferralToken
                    {
                        FeatureUserReferral = EnumFeatureUserReferral.DoneUnit1,
                        Token = userReferral.IsCoinRewarded ? _appSetting.UserReferralConfig?.DoneUnit1 ?? 0 : 0,
                        CreatedDate = DateTime.UtcNow,
                        PackageId = request.PackageId,
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

                var notificationContent = EnumNotificationContent.FriendCompletedExam;

                var tokenMission = EnumTokenMission.FriendCompletePT;

                if (request.FeatureUserReferral == EnumFeatureUserReferral.PT)
                {
                    tokenMission = EnumTokenMission.FriendCompletePT;
                    notificationContent = EnumNotificationContent.FriendCompletedExam;
                }
                else if (request.FeatureUserReferral == EnumFeatureUserReferral.DoneUnit1)
                {
                    tokenMission = EnumTokenMission.FriendCompleteUnit1;
                    notificationContent = EnumNotificationContent.FriendCompleteUnitOne;
                }
                else if (request.FeatureUserReferral == EnumFeatureUserReferral.Payment)
                {
                    tokenMission = EnumTokenMission.FriendCompletePayment;
                    notificationContent = EnumNotificationContent.FriendPaymentSuccessfully;
                }

                if (userReferral.IsCoinRewarded)
                {
                    var tokenHistories = new List<TokenHistoryQueueModel>()
                    {
                         new TokenHistoryQueueModel
                                    {
                                        VolatileToken = token,
                                        Type = EnumTokenHistoryType.Recevived,
                                        Feature = EnumTokenFeature.FriendMission,
                                        Mission = tokenMission,
                                        UserId = userReferral.SenderId,
                                        ObjectId = userReferral.ReceiverId
                                    }
                    };

                    if (isAddDoneUnit1)
                    {
                        tokenHistories.Add(new TokenHistoryQueueModel
                        {
                            VolatileToken = _appSetting.UserReferralConfig?.DoneUnit1 ?? 0,
                            Type = EnumTokenHistoryType.Recevived,
                            Feature = EnumTokenFeature.FriendMission,
                            Mission = EnumTokenMission.FriendCompleteUnit1,
                            UserId = userReferral.SenderId,
                            ObjectId = userReferral.ReceiverId
                        });
                    }

                    await _createTokenHistoryPublisher.Publish(tokenHistories, cancellationToken).ConfigureAwait(false);

                    var friendInformation = await _studentRepository.Queryable.Where(x => x.Human != null && x.Human.UserId == userReferral.ReceiverId).FirstOrDefaultAsync(cancellationToken);

                    await SendNotification(friendInformation?.Human?.FullName ?? string.Empty, token, userReferral.SenderId, userReferral.ReceiverId, notificationContent, cancellationToken);
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }

        private async Task SendNotification(string userName, int amountOfCoin, Guid userId, Guid senderId, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = new List<Guid> { userId },
                SenderId = senderId,
                Type = EnumNotificationType.LinkPage,
                Content = content,
                PlatformCode = EnumPlatformCode.LMS,
                ParamsMessage = new List<object> { userName, amountOfCoin },
                ParamsLink = new List<object>()
            };
            await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);
        }
    }
}
