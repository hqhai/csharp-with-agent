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
                var referralDiscountConfigs = await _referralDiscountConfigRepository.Queryable.OrderBy(x => x.IndexNumber).ToListAsync(cancellationToken);
                IList<ReferralDiscountConfig> listReferralDiscountConfig = new List<ReferralDiscountConfig>();
                if (request.SaveReferralDiscountConfigs != null)
                {
                    foreach (var item in request.SaveReferralDiscountConfigs)
                    {
                        var referralDiscountConfig = referralDiscountConfigs.FirstOrDefault(x => x.IndexNumber == item.IndexNumber);
                        if (referralDiscountConfig != null)
                        {
                            _mapper.Map(item, referralDiscountConfig);
                            referralDiscountConfig = _referralDiscountConfigRepository.Update(referralDiscountConfig);
                        }
                        else
                        {
                            referralDiscountConfig = _mapper.Map<ReferralDiscountConfig>(item);
                            referralDiscountConfig = _referralDiscountConfigRepository.Add(referralDiscountConfig);
                        }
                        if (!referralDiscountConfig.IsValid())
                        {
                            methodResult.AddErrorBadRequest(referralDiscountConfig.ErrorMessages);
                            return methodResult;
                        }
                        listReferralDiscountConfig.Add(referralDiscountConfig);
                    }
                    await _referralDiscountConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<ReferralDiscountConfigModel>>(listReferralDiscountConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
