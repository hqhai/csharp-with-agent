// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class GetFeatureAccessTimeBusinessQueryModel
    {
        public Guid UserId { get; set; }
        public Guid? CourseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
