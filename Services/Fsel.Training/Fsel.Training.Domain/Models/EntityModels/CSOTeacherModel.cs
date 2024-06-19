// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class CSOTeacherModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Phonenumber { get; set; }
        public string? Email { get; set; }
        public int CountClass { get; set; }
        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }
}
