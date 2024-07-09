// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class TechieModel : BaseModel
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public IList<TechieActionModel>? TechieActionModels { get; set; }
    }
}
