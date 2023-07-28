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
    using Microsoft.EntityFrameworkCore;

    public class SaveReferralDiscountConfigCommand : SaveListReferralDiscountConfigCommandModel, IRequest<MethodResult<IList<ReferralDiscountConfigModel>>>
    {
    }

    public class SaveReferralDiscountConfigCommandHandler : IRequestHandler<SaveReferralDiscountConfigCommand, MethodResult<IList<ReferralDiscountConfigModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IReferralDiscountConfigRepository _referralDiscountConfigRepository;

        public SaveReferralDiscountConfigCommandHandler(IMapper mapper, IReferralDiscountConfigRepository referralDiscountConfigRepository)
        {
            _mapper = mapper;
            _referralDiscountConfigRepository = referralDiscountConfigRepository;
        }

        public async Task<MethodResult<IList<ReferralDiscountConfigModel>>> Handle(SaveReferralDiscountConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ReferralDiscountConfigModel>>();

            await _referralDiscountConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var referralDiscountConfigs = await _referralDiscountConfigRepository.Queryable.ToListAsync(cancellationToken);

                if (request.SaveReferralDiscountConfigs != null)
                {
                    var isCheck = referralDiscountConfigs != null && referralDiscountConfigs.Count > 0;
                    referralDiscountConfigs?.Clear();
                    foreach (var item in request.SaveReferralDiscountConfigs)
                    {
                        var referralDiscountConfig = new ReferralDiscountConfig();
                        _mapper.Map(item, referralDiscountConfig);
                        referralDiscountConfigs?.Add(referralDiscountConfig);
                        if (!referralDiscountConfig.IsValid())
                        {
                            methodResult.AddErrorBadRequest(referralDiscountConfig.ErrorMessages);
                            return methodResult;
                        }
                    }
                    if (referralDiscountConfigs != null)
                    {
                        if (isCheck)
                        {
                            _referralDiscountConfigRepository.UpdateList(referralDiscountConfigs);
                        }
                        else
                        {
                            await _referralDiscountConfigRepository.AddList(referralDiscountConfigs);
                        }
                    }

                    await _referralDiscountConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<ReferralDiscountConfigModel>>(referralDiscountConfigs);
                return methodResult;
            });

            return methodResult;
        }
    }
}
