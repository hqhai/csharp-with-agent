// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumer
{
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.DictionaryAICmd;
    using Fsel.System.Application.Queues.Publisher;
    using MediatR;

    /// <summary>
    /// Consumer for Semantic Dictionary queue - calls Search/Generate and publishes result
    /// </summary>
    public class SemanticDictionaryConsumer : BaseConsumer<SemanticDictionaryAIQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly DictionaryPublisher _dictionaryPublisher;

        public SemanticDictionaryConsumer(
            AuthContext authContext,
            IMediator mediator,
            DictionaryPublisher dictionaryPublisher,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _dictionaryPublisher = dictionaryPublisher;
        }

        public override async Task ConsumeQueue(SemanticDictionaryAIQueueModel? message)
        {
            if (message == null || string.IsNullOrEmpty(message.UserId) || message.Request == null)
            {
                return;
            }

            try
            {
                // First, try to search for existing entry
                var searchCommand = new SearchDictionaryAICommand
                {
                    HighlightedItem = message.Request.HighlightedItem ?? "",
                    SentenceContext = message.Request.SentenceContext,
                    SourceLanguage = message.Request.SourceLanguage,
                    TargetLanguage = message.Request.TargetLanguage
                };

                var searchResult = await _mediator.Send(searchCommand, CancellationToken.None);
                SemanticDictionaryResultModel? result = null;

                if (searchResult != null && searchResult.IsOK && searchResult.Result != null)
                {
                    result = searchResult.Result;
                }
                else
                {
                    // If not found, generate new entry
                    var generateCommand = new GenerateDictionaryAICommand
                    {
                        HighlightedItem = message.Request.HighlightedItem ?? "",
                        SentenceContext = message.Request.SentenceContext,
                        SourceLanguage = message.Request.SourceLanguage,
                        TargetLanguage = message.Request.TargetLanguage
                    };

                    var generateResult = await _mediator.Send(generateCommand, CancellationToken.None);
                    if (generateResult != null && generateResult.IsOK)
                    {
                        result = generateResult.Result;
                    }
                }

                // Publish result back to Realtime
                if (result != null)
                {
                    var queueModel = new SemanticDictionaryQueueModel
                    {
                        UserId = message.UserId,
                        Result = result
                    };

                    await _dictionaryPublisher.PublishSemanticDictionary(queueModel, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                // Log error (in production, use proper logging)
                Console.WriteLine($"Error in SemanticDictionaryConsumer: {ex.Message}");
            }
        }
    }
}
