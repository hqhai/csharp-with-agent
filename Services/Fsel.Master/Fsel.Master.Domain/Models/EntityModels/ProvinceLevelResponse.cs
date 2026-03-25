// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    using System.Collections.Generic;

    public class ProvinceLevelResponse
    {
        public IList<ProvinceLevelModels> AllLevels { get; set; } = new List<ProvinceLevelModels>();
        public IList<ProvinceLevelsModel> Data { get; set; } = new List<ProvinceLevelsModel>();
    }
}
