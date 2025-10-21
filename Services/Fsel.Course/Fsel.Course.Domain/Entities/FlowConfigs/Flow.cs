// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.FlowConfigs
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class Flow : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public EnumFlowType Type { get; set; }

        [Range(0, 150, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int FromAge { get; set; }

        [Range(1, 150, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int ToAge { get; set; }

        public EnumStatus Status { get; set; }

        /// <summary>
        /// AiConfigSetting
        /// </summary>
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public Category? Category { get; set; }
        public Guid ProgramId { get; set; }
        public ICollection<PlacementTestGroupResult> PlacementTestGroupResults { get; set; } = new List<PlacementTestGroupResult>();
        public ICollection<StepFlow> StepFlows { get; set; } = new List<StepFlow>();

        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
    }
}
