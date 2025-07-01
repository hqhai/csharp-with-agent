// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.V1i1;

    public class Document : Entity
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

        public ICollection<LessonModule> LessonModules { get; set; } = new List<LessonModule>();
    }
}
