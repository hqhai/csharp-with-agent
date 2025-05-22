// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Queries.VoucherQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Vouchers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportDetailHistoryVoucherAutoCommand : SearchDetailHistoryVoucherAutoQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportDetailHistoryVoucherAutoCommandHandler : IRequestHandler<ExportDetailHistoryVoucherAutoCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ExportDetailHistoryVoucherAutoCommandHandler(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<MethodResult<Stream>> Handle(ExportDetailHistoryVoucherAutoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var query = new SearchDetailHistoryVoucherAutoQuery
            {
                Keyword = request.Keyword,
                Status = request.Status,
                Day = request.Day,
                CodePrefix = request.CodePrefix,
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
            var exportModels = _mapper.Map<IList<ExportHistoryVoucherAutoModel>>(result);
            var stream = exportModels.ExportExcel();
            methodResult.Result = stream;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
