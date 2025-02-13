// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class CreateStudentsToEventFromByteModel
    {
        public Guid DistrictId { get; set; }
        public Guid SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? Key { get; set; }
        public byte[]? File { get; set; }
    }
}
