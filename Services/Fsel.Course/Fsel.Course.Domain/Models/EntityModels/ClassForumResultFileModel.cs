// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;

    public class ClassForumResultFileModel
    {
        public string? FilePath { get; set; }
        public Guid ClassForumResultId { get; set; }

        public ClassForumResultModel? ClassForumResult { get; set; }
    }
}
