// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class CreateStudentsToEventFromFileModel
    {
        public byte[]? File { get; set; }
        public int? NumberOfStudent { get; set; }
        public string? Message { get; set; }
        public string? Key { get; set; }
        public int? StatusCode { get; set; }
    }
}
