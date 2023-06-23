// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class CourseResultModel : BaseModel
    {
        /// <summary>
        /// Lưu kết quả của Couse
        /// </summary>
        [MinLength(0, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Result { get; set; }

        /// <summary>
        /// Trạng thái của Couse
        /// </summary>
        public EnumCourseStatus Status { get; set; }
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
    }
}
