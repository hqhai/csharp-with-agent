// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class ExtraPracticeChapterModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PageNumber { get; set; }
        public Guid ExtraPracticeId { get; set; }
    }
}
