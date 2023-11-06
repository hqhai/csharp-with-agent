// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentGameInfoModel : BaseModel
    {
        public EnumGameCourseLevel CourseLevel { get; set; }
        public Guid StudentId { get; set; }

        public string? NickName { get; set; }
        public EnumGender Gender { get; set; }

        public int Level { get; set; }

        public Guid AvatarImageId { get; set; }

        public Guid TagNameId { get; set; }

        public string? TagName { get; set; }
        public int? HighestRoundNumber { get; set; }
        public long? HighestScore { get; set; }

        public StudentTagNameModel? StudentTagName { get; set; }

    }
}
