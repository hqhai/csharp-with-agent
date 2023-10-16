// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SystemService.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;

    public class ForbiddenWordModel: BaseModel
    {
        public string? Word { get; set; }

        public string? Description { get; set; }
    }
}
