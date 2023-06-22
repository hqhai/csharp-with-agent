// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchClassLiveByCsoQueryModel : BaseQueryModel
    {
        public string? Code { get; set; }

        public string? ClassName { get; set; }
    }
}
