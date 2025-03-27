// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class CompetitionEventTreeModel
    {
        public Guid Id { get; set; }

        public string? EventCode { get; set; }

        public string? Name { get; set; }

        public IList<CompetitionEventTreeModel>? Childents { get; set; }
    }
}
