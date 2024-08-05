// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{

    public class SchoolStudent
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Grade { get; set; }
        public string? ParentEmail { get; set; }

        public Guid StudentId { get; set; }

        public Guid UserId { get; set; }
    }
}
