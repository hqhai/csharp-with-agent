// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.DisplayOrderConfigs
{
    public class UpdateDisplayOrderConfigCommandModel
    {
        public Guid Id { get; set; }

        public int DisplayOrder { get; set; }

        public bool Status { get; set; }
    }

    public class UpdateDisplayOrderConfigsCommandModel
    {
        public IList<UpdateDisplayOrderConfigCommandModel>? UpdateDisplayOrderConfigs { get; set; }
    }
}
