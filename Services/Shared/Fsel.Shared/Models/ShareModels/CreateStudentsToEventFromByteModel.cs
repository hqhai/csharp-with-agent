// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class CreateStudentsToEventFromByteModel
    {
        public EnumCompetitionEventCategory Category { get; set; }
        public string? Key { get; set; }
        public byte[]? File { get; set; }
    }
}
