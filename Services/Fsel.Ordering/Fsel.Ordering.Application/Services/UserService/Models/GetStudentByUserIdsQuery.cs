// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using System.Collections.Generic;

    public class GetStudentByUserIdsQuery
    {
        public IList<Guid>? UserIds { get; set; }
    }
}
