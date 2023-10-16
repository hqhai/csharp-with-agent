// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices.Model
{
    public class GetStatusByUserCommandModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public Guid ClassId { get; set; }
        public Guid PackageId { get; set; }
    }
}
