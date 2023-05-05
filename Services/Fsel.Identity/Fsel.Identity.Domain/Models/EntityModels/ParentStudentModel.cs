// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ParentStudentModel : BaseModel
    {
        public Guid? ParentId { get; set; }
        public Guid? StudentId { get; set; }
        public StudentModel? Student { get; set; }
    }
}
