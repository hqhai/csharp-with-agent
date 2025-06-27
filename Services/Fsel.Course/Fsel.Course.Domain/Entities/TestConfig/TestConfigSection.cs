// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfig
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TestConfigSection : Entity
    {
        /// <summary>
        /// Tên Section
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        //public string? MediaPost { get; set; }     ------------------------------------> Add to Config
        //[MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        //public string? VideoFilePath { get; set; } ------------------------------------> Add to Config

        //[MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        //public string? SubFilePath { get; set; }   ------------------------------------> Add to Config

        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int TargetWord { get; set; }

        public int DisplayOrder { get; set; }

        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? ExecutionTime { get; set; }

        public EnumTestLayoutType LayoutType { get; set; }
        public double? TotalScore { get; set; }

        public Guid? SkillId { get; set; }
        public Skill? Skill { get; set; }

        public Guid? TestConfigId { get; set; }
        public TestConfig? TestConfig { get; set; }

        public Guid? ParentId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }
    }
}
