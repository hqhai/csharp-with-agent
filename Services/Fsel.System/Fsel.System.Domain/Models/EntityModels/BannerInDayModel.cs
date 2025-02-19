// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    public class BannerInDayModel
    {
        public DateTime Date { get; set; }

        public bool IsBanner { get; set; }

        public bool IsHeading { get; set; }

        public bool IsHome { get; set; }

        public bool IsLeft { get; set; }
    }
}
