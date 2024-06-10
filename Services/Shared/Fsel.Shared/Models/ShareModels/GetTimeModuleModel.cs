// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class GetTimeModuleModel
    {
        public Guid UserId { get; set; }
        public double RemainingTime { get; set; }
        public double WorkingTime { get; set; }
    }
}
