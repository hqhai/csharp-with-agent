// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.BannerStudents
{
    public class CreateBannerStudentCommandModel
    {
        public Guid StudentId { get; set; }

        public Guid BannerId { get; set; }
    }
}
