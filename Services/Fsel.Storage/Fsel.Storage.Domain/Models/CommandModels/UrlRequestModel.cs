// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Domain.Models.CommandModels
{
    using Fsel.Storage.Domain.Enums;

    public class UrlRequestModel
    {
        public string? Url { get; set; }

        public EnumBucketType? BucketType { get; set; }
    }
}
