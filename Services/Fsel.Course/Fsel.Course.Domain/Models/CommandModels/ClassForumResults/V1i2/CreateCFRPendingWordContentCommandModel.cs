// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults.V1i2
{
    using Microsoft.AspNetCore.Http;

    public class CreateCFRPendingWordContentCommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public IFormFile? FormFile { get; set; }

        public string? Content { get; set; }
    }
}
