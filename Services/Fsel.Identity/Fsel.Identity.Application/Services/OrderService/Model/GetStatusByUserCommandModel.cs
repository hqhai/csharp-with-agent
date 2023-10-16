// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class GetStatusByUserCommandModel
    {
        public Guid? UserId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? PackageId { get; set; }
    }
}
