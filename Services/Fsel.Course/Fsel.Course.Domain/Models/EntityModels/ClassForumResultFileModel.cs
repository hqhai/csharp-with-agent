// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Helpers;

    public class ClassForumResultFileModel
    {
        public bool IsRetry { get; set; }

        public string? FilePath { get; set; }

        public int? TimeCount => !string.IsNullOrEmpty(FilePath) ? MediaHelper.GetMediaDurationAsync(FilePath) : null;
    }
}
