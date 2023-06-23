// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchTeacherFreeDatesByCsoQueryModel : BaseQueryModel
    {
        public DateTime? EndTime { get; set; }

        public DateTime? StartTime { get; set; }

        public Guid TeacherId { get; set; }
    }
}
