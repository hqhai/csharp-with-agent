// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CategoryModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public EnumTypeCategory Type { get; set; }

        public EnumStatus Status { get; set; }

        public Guid? ParentId { get; set; }

        public IList<SubjectConditionModel>? SubjectConditions { get; set; }
    }
}
