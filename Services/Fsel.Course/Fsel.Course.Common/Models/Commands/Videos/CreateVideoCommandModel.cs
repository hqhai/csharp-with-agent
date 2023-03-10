using Fsel.Course.Common.Models.Commands.VideoTimeCode;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.Videos
{
    public class CreateVideoCommandModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid TeacherId { get; set; } = Guid.Empty;

        public EnumCourseLevel CourseLevel { get; set; }
        public List<CreateVideoTimeCodeCommandModel>? VideoTimeCodes { get; set; } = new List<CreateVideoTimeCodeCommandModel>();
    }
}