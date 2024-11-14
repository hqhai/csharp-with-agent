// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.OtherCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplateToolAddCoinEventCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateToolAddCoinEventCommandHandler : IRequestHandler<ExportTemplateToolAddCoinEventCommand, MethodResult<Stream>>
    {
        public ExportTemplateToolAddCoinEventCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateToolAddCoinEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<Stream> methodResult = new MethodResult<Stream>();
            var data = new List<ImportCoinEventStudentModel>();
            methodResult.Result = data.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
