// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.SupportTickets
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchSupportTicketQueryModel : BaseQueryModel
    {
        public Guid? SupportCategoryId { get; set; }
        public Guid? SupportQuestionId { get; set; }
        public EnumSupportTicketStatus? Status { get; set; }
    }
}
