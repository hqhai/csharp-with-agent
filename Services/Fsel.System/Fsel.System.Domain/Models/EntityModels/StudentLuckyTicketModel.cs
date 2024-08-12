// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    public class StudentLuckyTicketModel
    {
        public Guid StudentId { get; set; }
        public string? AvatarPath { get; set; }
        public string? StudentName { get; set; }
        public string? Email { get; set; }
        public string? SchoolName { get; set; }
        public string? Ticket { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
