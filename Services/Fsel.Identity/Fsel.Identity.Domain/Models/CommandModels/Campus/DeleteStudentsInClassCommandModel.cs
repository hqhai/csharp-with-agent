// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Campus
{
    public class DeleteStudentsInClassCommandModel
    {
        public Guid SchoolClassId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}
