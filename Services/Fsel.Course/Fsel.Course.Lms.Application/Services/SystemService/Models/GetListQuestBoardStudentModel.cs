// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;

    public class GetListQuestBoardStudentModel
    {
        public Guid? QuestBoardId { get; set; }

        public Guid? StudentId { get; set; }
    }
}
