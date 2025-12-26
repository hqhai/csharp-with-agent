// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Documents
{
    using Fsel.Course.Domain.Entities;

    public class CreateDocumentCommandModel
    {
        public IList<DocumentFile>? Files { get; set; }
    }
}
