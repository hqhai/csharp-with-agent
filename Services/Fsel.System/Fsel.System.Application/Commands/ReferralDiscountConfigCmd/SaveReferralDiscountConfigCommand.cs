// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ReferralDiscountConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ReferralDiscountConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveReferralDiscountConfigCommand : SaveReferralDiscountConfigCommandModel, IRequest<MethodResult<ReferralDiscountConfigModel>>
    {
    }

    public class SaveReferralDiscountConfigCommandHandler : IRequestHandler<SaveReferralDiscountConfigCommand, MethodResult<ReferralDiscountConfigModel>>
    {
        private readonly IMapper _mapper;
        private readonly IReferralDiscountConfigRepository _referralDiscountConfigRepository;

        public SaveReferralDiscountConfigCommandHandler(IMapper mapper, IReferralDiscountConfigRepository referralDiscountConfigRepository)
        {
            _mapper = mapper;
            _referralDiscountConfigRepository = referralDiscountConfigRepository;
        }

        public async Task<MethodResult<ReferralDiscountConfigModel>> Handle(SaveReferralDiscountConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReferralDiscountConfigModel>();

            await _referralDiscountConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var referralDiscountConfig = await _referralDiscountConfigRepository.GetByIdAsync(request.Id);

                if (referralDiscountConfig != null)
                {
                    _mapper.Map(request, referralDiscountConfig);
                    referralDiscountConfig = _referralDiscountConfigRepository.Update(referralDiscountConfig);
                }
                else
                {
                    referralDiscountConfig = _mapper.Map<ReferralDiscountConfig>(request);
                    referralDiscountConfig = _referralDiscountConfigRepository.Add(referralDiscountConfig);
                }
                if (!referralDiscountConfig.IsValid())
                {
                    methodResult.AddErrorBadRequest(referralDiscountConfig.ErrorMessages);
                    return methodResult;
                }

                await _referralDiscountConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ReferralDiscountConfigModel>(referralDiscountConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
