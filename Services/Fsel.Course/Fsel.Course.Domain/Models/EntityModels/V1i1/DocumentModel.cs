// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class DocumentModel : BaseModel
    {
        public IList<DocumentFileModel>? Files { get; set; }
    }

    public class DocumentFileModel
    {
        public EnumDocumentType Type { get; set; }

        public string? Name { get; set; }

        public string? FilePath { get; set; }
    }
}
