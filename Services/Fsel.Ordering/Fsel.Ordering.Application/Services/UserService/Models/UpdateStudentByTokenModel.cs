// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using System;

    public class UpdateStudentByTokenModel
    {
        public Guid StudentId { get; set; }
        public long NumberOfToken { get; set; }
    }
}
