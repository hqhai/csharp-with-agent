// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports.Sales
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Queues.Publishers.ExportFiles;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.QueueModels;
    using MediatR;

    public class GetFileExcelUserInformationSupportSaleQuery : IRequest<MethodResult<string>>
    {
    }

    public class GetFileExcelUserInformationSupportSaleQueryHandler : IRequestHandler<GetFileExcelUserInformationSupportSaleQuery, MethodResult<string>>
    {
        private readonly ExportFileUserInformationSupportSalePublisher _exportFileUserInformationSupportSalePublisher;

        public GetFileExcelUserInformationSupportSaleQueryHandler(ExportFileUserInformationSupportSalePublisher exportFileUserInformationSupportSalePublisher)
        {
            _exportFileUserInformationSupportSalePublisher = exportFileUserInformationSupportSalePublisher;
        }

        public async Task<MethodResult<string>> Handle(GetFileExcelUserInformationSupportSaleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            string fileName = $"ExportReportSaleSupport_{DateTime.Now.Ticks}.xlsx";
            await _exportFileUserInformationSupportSalePublisher.Publish(new ExportUserInformationSupportSaleQueueModel
            {
                FileName = fileName
            }, cancellationToken);

            methodResult.Result = ValueSettings.FSEL_PUBLIC_FILES_URL + fileName;
            return methodResult;
        }
    }
}
