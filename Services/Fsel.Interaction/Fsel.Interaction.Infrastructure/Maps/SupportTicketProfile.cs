// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportTickets;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class SupportTicketProfile : Profile
    {
        public SupportTicketProfile()
        {
            CreateMap<SupportTicket, SupportTicketModel>().IgnoreAllNonExisting();
            CreateMap<UpdateStatusSupportTicketCommandModel, SupportTicket>().IgnoreAllNonExisting();
        }
    }
}
