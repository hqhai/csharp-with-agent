// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    public class CreateOrdersForStudentCampusCommandModels
    {
        public IList<CreateOrdersForStudentCampusCommandModel>? Students { get; set; }
    }

    public class CreateOrdersForStudentCampusCommandModel
    {
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public string? StudentCode { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
