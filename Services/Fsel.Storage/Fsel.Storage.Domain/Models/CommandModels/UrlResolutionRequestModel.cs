// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Domain.Models.CommandModels
{
    using Fsel.Storage.Domain.Enums;

    public class UrlResolutionRequestModel : UrlRequestModel
    {
        public EnumBucketType? BucketType { get; set; }
    }
}
