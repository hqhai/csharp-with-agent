// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers.ExportFiles
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.QueueModels;

    public class ExportFileUserInformationSupportSalePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ExportFileUserInformationSupportSalePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ExportUserInformationSupportSaleQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.ExportExcelUserInformationSupportSale, request, cancellationToken);
        }
    }
}
