// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.VideoTimeCodeResults
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.EntityModels;

    public class VideoTimeCodeResultByStudentModel : VideoTimeCodeResultModel
    {
        public VideoTimeCodeResultModel? VideoTimeCodeResultCurrentStudent { get; set; }

        public IList<VideoTimeCodeResultModel>? VideoTimeCodeResultAllStudents { get; set; }
    }
}
