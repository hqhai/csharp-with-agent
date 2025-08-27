// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers.ExportFiles
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Queries.Reports.Sales;
    using Fsel.Shared.Models.ShareModels.QueueModels;
    using MediatR;

    public class ExportFileUserInformationSupportSaleConsumer : BaseConsumer<ExportUserInformationSupportSaleQueueModel>
    {
        private readonly IMediator _mediator;

        public ExportFileUserInformationSupportSaleConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ExportUserInformationSupportSaleQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new ExportUserInformationSupportSaleQuery
            {
                FileName = message.FileName
            }).ConfigureAwait(false);
        }
    }
}
