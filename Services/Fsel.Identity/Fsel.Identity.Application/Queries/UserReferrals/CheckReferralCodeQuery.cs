// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;

    public class CheckReferralCodeQuery : IRequest<MethodResult<SenderModel>>
    {
        public string? ReferralCode { get; set; }
    }

    public class CheckReferralCodeQueryHandler : IRequestHandler<CheckReferralCodeQuery, MethodResult<SenderModel>>
    {
        private readonly IMediator _mediator;

        public CheckReferralCodeQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<SenderModel>> Handle(CheckReferralCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SenderModel>();

            var result = await _mediator.Send(new GetSenderByCodeQuery() { ReferralCode = request.ReferralCode }, cancellationToken);

            methodResult.Result = result.Result;
            return methodResult;
        }
    }
}
