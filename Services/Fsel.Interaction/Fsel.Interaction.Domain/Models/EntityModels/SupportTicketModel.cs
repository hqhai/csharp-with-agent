// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SupportTicketModel : BaseModel
    {
        public string? Code { get; set; }

        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserCode { get; set; }

        public string? Email { get; set; }

        public string? Content { get; set; }

        public IList<string>? FilePaths { get; set; }

        public string? OtherProblem { get; set; }

        public EnumSupportTicketStatus Status { get; set; }

        public Guid? SupportCategoryId { get; set; }

        public Guid? SupportQuestionId { get; set; }

        public string? QuestionName { get; set; }
    }
}
