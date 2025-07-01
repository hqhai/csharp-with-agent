// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Documents
{
    using Fsel.Course.Domain.Enums;

    public class CreateDocumentCommandModel
    {
        public IList<CreateDocumentFileModel>? Files { get; set; }
    }

    public class CreateDocumentFileModel
    {
        public EnumDocumentType Type { get; set; }

        public string? Name { get; set; }

        public string? FilePath { get; set; }
    }
}
