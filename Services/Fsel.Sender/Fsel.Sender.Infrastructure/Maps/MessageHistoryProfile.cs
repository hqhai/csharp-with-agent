// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Shared.Models.SenderTemplates;

    public class MessageHistoryProfile : Profile
    {
        public MessageHistoryProfile()
        {
            CreateMap<SaveMessageHistoryByTypeEmailCommandModel, MessageHistory>().IgnoreAllNonExisting();
            CreateMap<MessageHistory, HistorySendMailLearningProgressModel>().IgnoreAllNonExisting();
        }
    }
}
