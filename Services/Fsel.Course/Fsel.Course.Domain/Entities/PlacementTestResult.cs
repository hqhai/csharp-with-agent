// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;

    public class PlacementTestResult : BaseResultScore
    {
        public EnumPlacementTestLevel Level { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
    }
}
