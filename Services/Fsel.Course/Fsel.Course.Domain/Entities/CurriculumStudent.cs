// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class CurriculumStudent : Entity
    {
        /// <summary>
        /// ID giáo trình
        /// </summary>
        public Guid CurriculumId { get; set; }

        public CurriculumConfig? Curriculum { get; set; }

        /// <summary>
        /// ID học viên
        /// </summary>
        public Guid StudentId { get; set; }
    }
}
