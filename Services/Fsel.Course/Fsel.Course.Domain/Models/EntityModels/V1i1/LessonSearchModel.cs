// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class LessonSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public EnumStatus Status { get; set; }

        public Guid? LevelId { get; set; }

        public string? NameLevel { get; set; }

        public Guid? ProgramId { get; set; }

        public string? NameProgram { get; set; }

        public string? Overview { get; set; }

        public IList<string>? Skills { get; set; }

        public IList<VideoSearchModel>? Videos { get; set; }

        public Guid OriginalId { get; set; }
    }

    public class VideoSearchModel
    {
        public Guid? TeacherId { get; set; }

        public Guid VideoId { get; set; }

        public string? NameTeacher { get; set; }

        public IList<EnumTimeCodeType>? TimeCodeTypes { get; set; }
    }
}
