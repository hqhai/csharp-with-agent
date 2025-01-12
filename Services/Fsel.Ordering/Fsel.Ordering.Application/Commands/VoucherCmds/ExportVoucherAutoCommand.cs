// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Queries.VoucherQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Vouchers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportVoucherAutoCommand : SearchVoucherQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportVoucherAutoCommandHandler : IRequestHandler<ExportVoucherAutoCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ExportVoucherAutoCommandHandler(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<MethodResult<Stream>> Handle(ExportVoucherAutoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var query = new SearchVoucherAutoQuery
            {
                Keyword = request.Keyword,
                Status = request.Status,
                CreatedDate = request.CreatedDate,
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
            var exportModels = _mapper.Map<IList<ExportVoucherModel>>(result);
            var stream = exportModels.ExportExcel();
            methodResult.Result = stream;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
