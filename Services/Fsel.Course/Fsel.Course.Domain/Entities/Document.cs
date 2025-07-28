// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums;
    using Fsel.Core.Entities;

    public class Document : Entity, IVersionEntity
    {
        public string? FilesStr { get; set; }

        [NotMapped]
        public IList<DocumentFile>? Files
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<IList<DocumentFile>>(FilesStr);
            }
            set { FilesStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public Guid OriginalId { get; set; }

        public int Version { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }
    }
}
