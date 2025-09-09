// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class CurriculumStudent : Entity
    {
        /// <summary>
        /// ID giáo trình
        /// </summary>
        public Guid? CurriculumId { get; set; }

        public CurriculumConfig? Curriculum { get; set; }

        /// <summary>
        /// ID học viên
        /// </summary>
        public Guid? StudentId { get; set; }

        /// <summary>
        /// Trạng thái học viên trong giáo trình
        /// </summary>
        public EnumCurriculumStudent CurriculumStudentStatus { get; set; }

    }
}
