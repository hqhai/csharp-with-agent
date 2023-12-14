// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class GetTokenQueryModel
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
    }
}
