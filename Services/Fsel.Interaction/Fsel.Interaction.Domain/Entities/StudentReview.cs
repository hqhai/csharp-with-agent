// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class StudentReview : Entity
    {
        /// <summary>
        /// Review Type
        /// </summary>
        public EnumReviewType ReviewType { get; set; }

        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public ICollection<StudentReviewDetail> StudentReviewDetails { get; set; } = new List<StudentReviewDetail>();
    }
}
