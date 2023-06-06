// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters
{
    public class UpdateExtraPracticeChapterCommand
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PageNumber { get; set; }
    }
}
