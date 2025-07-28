// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class PlacementTest : Entity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumPlacementTestLevel PlacementTestLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        [NotMapped]
        public double ExecutionTime
        {
            get
            {
                return PlacementTestSections.Select(x => x.SectionGroup).Sum(x => x!.ExecutionTime);
            }
        }

        public Level? Level { get; set; }
        public Guid? LevelId { get; set; }
        public Category? Program { get; set; }
        public Guid? ProgramId { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public ICollection<PlacementTestSection> PlacementTestSections { get; set; } = new List<PlacementTestSection>();
        public ICollection<PlacementTestResult> PlacementTestResults { get; set; } = new List<PlacementTestResult>();
    }
}
