// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;

    public class DocumentStudentProgressModel
    {
        public IList<DocumentFile>? Files { get; set; }
        public int DisplayOrder { get; set; }
        public long TimeSpent { get; set; }
        public DateTime? LastVisited { get; set; }
        public int Visit { get; set; }
        public EnumResultStatus Status { get; set; }

        public IList<DocumentFile>? FilePaths
        {
            get { return Files?.Where(p => p.Type == EnumDocumentType.File).ToList(); }
        }

        public IList<DocumentFile>? Links
        {
            get { return Files?.Where(p => p.Type == EnumDocumentType.Link).ToList(); }
        }
    }
}
