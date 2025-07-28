// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults.V1i1
{
    using Microsoft.AspNetCore.Http;

    public class CreateCFRPendingWordContentCommandModel
    {
        public Guid LessonResultId { get; set; }

        public IFormFile? FormFile { get; set; }

        public string? Content { get; set; }
    }
}
