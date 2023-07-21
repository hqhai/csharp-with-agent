// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportTickets
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateStatusSupportTicketCommandModel : BaseCommandModel
    {
        public EnumSupportTicketStatus Status { get; set; }
    }
}
