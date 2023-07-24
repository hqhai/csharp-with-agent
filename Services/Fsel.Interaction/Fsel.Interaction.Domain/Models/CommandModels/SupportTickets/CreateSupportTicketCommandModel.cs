// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportTickets
{
    using System;
    using System.Collections.Generic;

    public class CreateSupportTicketCommandModel
    {
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserCode { get; set; }

        public string? Email { get; set; }

        public string? Content { get; set; }

        public IList<string>? FilePaths { get; set; }

        public string? OtherProblem { get; set; }

        public Guid? SupportCategoryId { get; set; }

        public Guid? SupportQuestionId { get; set; }
    }
}
