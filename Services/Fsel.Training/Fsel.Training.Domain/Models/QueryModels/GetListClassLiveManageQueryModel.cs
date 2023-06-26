// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class GetListClassLiveManageQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }
        public string? ClassName { get; set; }
        public string? ClassCode { get; set; }
    }
}
