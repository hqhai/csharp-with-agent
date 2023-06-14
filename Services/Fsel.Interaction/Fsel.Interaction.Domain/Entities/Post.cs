// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Post : Entity
    {
        /// <summary>
        /// Title
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Title { get; set; }

        /// <summary>
        /// Content
        /// </summary>
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public EnumPostStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Đường dẫn tệp
        /// </summary>

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? FilePathsStr { get; set; }

        [NotMapped]
        public IList<string>? FilePaths
        {
            get { return ConvertHelper.Deserialize<IList<string>>(FilePathsStr); }
            set { FilePathsStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
