// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    public class StudentSurveyRecordModel
    {
        public Guid UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? StudentCode { get; set; }
    }
}
