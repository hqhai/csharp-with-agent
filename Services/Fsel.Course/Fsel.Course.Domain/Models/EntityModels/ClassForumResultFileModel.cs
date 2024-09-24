// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;

    public class ClassForumResultFileModel
    {
        private string? _filePath;

        public string? FilePath
        {
            set { _filePath = value; }
            get { return _filePath.AddS3BaseUrl(); }
        }

        public int? TimeCount { get; set; }
    }
}
