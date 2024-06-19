// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Domain.Models.CommandModels
{
    using Fsel.Shared.Enums;

    public class UrlResolutionRequestModel : UrlRequestModel
    {
        public EnumBucketType? BucketType { get; set; }
    }
}
