// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class GetListClassLiveManageQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }
        public Guid? ClassId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
