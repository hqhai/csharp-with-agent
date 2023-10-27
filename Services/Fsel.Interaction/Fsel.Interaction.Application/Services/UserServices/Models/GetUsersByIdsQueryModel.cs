// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GetUsersByIdsQueryModel
    {
        public IList<Guid>? UserIds { get; set; }
    }
}
