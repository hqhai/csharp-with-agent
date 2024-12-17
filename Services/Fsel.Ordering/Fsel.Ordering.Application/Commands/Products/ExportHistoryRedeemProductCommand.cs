// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Queries.Products;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Products;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportHistoryRedeemProductCommand : SearchHistoryRedeemByAdminQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportHistoryRedeemProductCommandHandler : IRequestHandler<ExportHistoryRedeemProductCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ExportHistoryRedeemProductCommandHandler(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<MethodResult<Stream>> Handle(ExportHistoryRedeemProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var query = new SearchHistoryRedeemByAdminQuery
            {
                Keyword = request.Keyword,
            };
            query.SetIsQueryAll(true);

            var queryResult = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
            if (!queryResult.IsOK)
            {
                methodResult.AddError(queryResult.StatusCode, queryResult.ErrorMessages);
                return methodResult;
            }
            if (queryResult.Result == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ExportFaile));
                return methodResult;
            }
            var result = queryResult.Result.Items;

            var exportModels = _mapper.Map<IList<ExportHistoryRedeemProductModel>>(result);
            var stream = exportModels.ExportExcel();
            methodResult.Result = stream;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
