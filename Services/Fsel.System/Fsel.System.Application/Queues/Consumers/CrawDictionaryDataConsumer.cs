// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.System.Application.Commands.DictionaryCmd;
    using MediatR;

    public class CrawDictionaryDataConsumer : BaseConsumer<string>
    {
        private readonly IMediator _mediator;

        public CrawDictionaryDataConsumer(AuthContext authContext, IMediator mediator) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(string? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new CrawDictionaryCommand
            {
                Word = message
            }).ConfigureAwait(false);
        }
    }
}
