// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models
{
    using System;

    public class UpdateStudentByTokenModel
    {
        public Guid StudentId { get; set; }
        public long NumberOfToken { get; set; }
    }
}
