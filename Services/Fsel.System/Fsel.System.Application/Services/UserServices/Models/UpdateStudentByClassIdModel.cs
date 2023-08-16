// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models
{
    using System;

    public class UpdateStudentByClassIdModel
    {
        public Guid? ClassId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? PackageId { get; set; }
    }
}
