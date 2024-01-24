// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.PayooQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Models.EntityModels.Payoo;
    using MediatR;

    public class ShopBackUrlQuery : ShopBackUrlModel,IRequest<MethodResult<ShopBackUrlModel>>
    {
    }
    public class ShopBackUrlQueryHandler : IRequestHandler<ShopBackUrlQuery, MethodResult<ShopBackUrlModel>>
    {
        public async Task<MethodResult<ShopBackUrlModel>> Handle(ShopBackUrlQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ShopBackUrlModel>();
            methodResult.Result = request;
            return methodResult;
        }
    }
}
