// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.Flags
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchFlagQueryModel : BaseQueryModel
    {
        public EnumInteractionType? Type { get; set; }
    }
}
