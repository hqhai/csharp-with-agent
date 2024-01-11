// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;

    public class CourseTeacherModel : BaseModel
    {
        public Guid TeacherId { get; set; }

        public string? FullName { get; set; }

        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public Guid CourseId { get; set; }

        public string? Nationality { get; set; }

        public string? Deggree { get; set; }

        public string? Experience { get; set; }

        public string? Strength { get; set; }
    }
}
