// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models
{
    public class UpdateStudentByTokenModel
    {
        public Guid StudentId { get; set; }
        public long NumberOfToken { get; set; }
    }
}
