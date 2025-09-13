// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using Fsel.Core.Base.BaseModels;

    public class StudentCampusModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Class { get; set; }
        public int TotalLessonDone { get; set; }
        public int TotalLesson { get; set; }
        public Guid StudentId { get; set; }
    }
}
