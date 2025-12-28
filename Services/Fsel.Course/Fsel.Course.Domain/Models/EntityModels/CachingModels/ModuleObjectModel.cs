// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.CachingModels
{
    public class ModuleObjectModel
    {
        public Guid ObjectId { get; set; }
        public string? Type { get; set; }
        public int TotalQuestion { get; set; }
        public int TotalCorrect { get; set; }
        public IList<ModuleObjectModel> Modules { get; set; } = new List<ModuleObjectModel>();
    }
}
