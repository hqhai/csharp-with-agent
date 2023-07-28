// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchTeacherFreeDatesByCsoQueryModel : BaseQueryModel
    {
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public Guid? TeacherId { get; set; }
    }
}
