// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ReferralDiscountConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListReferralDiscountConfigQuery : IRequest<MethodResult<IList<ReferralDiscountConfigModel>>>
    {
    }

    public class GetListReferralDiscountConfigQueryHandler : IRequestHandler<GetListReferralDiscountConfigQuery, MethodResult<IList<ReferralDiscountConfigModel>>>
    {
        private readonly IReferralDiscountConfigRepository _referralDiscountConfigRepository;

        public GetListReferralDiscountConfigQueryHandler(IReferralDiscountConfigRepository referralDiscountConfigRepository)
        {
            _referralDiscountConfigRepository = referralDiscountConfigRepository;
        }

        public async Task<MethodResult<IList<ReferralDiscountConfigModel>>> Handle(GetListReferralDiscountConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ReferralDiscountConfigModel>>();

            var referralDiscountConfig = await _referralDiscountConfigRepository.Queryable.OrderBy(x => x.IndexNumber)
                                    .Select(x => new ReferralDiscountConfigModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        IndexNumber = x.IndexNumber,
                                        RecevicerDiscountType = x.RecevicerDiscountType,
                                        RecevierDiscountValue = x.RecevierDiscountValue,
                                        SenderDiscountType = x.SenderDiscountType,
                                        SenderDiscountValue = x.SenderDiscountValue,
                                    }).ToListAsync(cancellationToken);
            methodResult.Result = referralDiscountConfig;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
