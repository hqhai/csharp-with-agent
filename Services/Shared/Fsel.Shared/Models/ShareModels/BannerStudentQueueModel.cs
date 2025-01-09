// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;

    public class BannerStudentQueueModel
    {
        public Guid BannerId { get; set; }

        public string? Name { get; set; }

        public string? Content { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumBannerType Type { get; set; }

        public bool MixPanel { get; set; }

        public IList<BannerImageModel>? BannerImages { get; set; }
    }

    public class BannerStudentsQueueModel
    {
        public Guid UserId { get; set; }

        public IList<BannerStudentQueueModel>? BannerStudents { get; set; }
    }
}
