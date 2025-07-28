// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.System.Application.Queries.DictionaryQuery;
    using MediatR;

    public class DictionaryConsumer : BaseConsumer<string>
    {
        private readonly IMediator _mediator;

        public DictionaryConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(string? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new GetDictionaryByWordVer2Query
            {
                Word = message
            }).ConfigureAwait(false);
        }
    }
}
