// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.Models.Commands;

    public class MessageHistoryProfile : Profile
    {
        public MessageHistoryProfile()
        {
            CreateMap<SaveMessageHistoryByTypeEmailCommandModel, MessageHistory>().IgnoreAllNonExisting();
        }
    }
}
