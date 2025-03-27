// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.CommandModels
{
    public class CreateBlindBoxesCommandModel
    {
        public IList<Guid>? UserIds { get; set; }
    }
}
