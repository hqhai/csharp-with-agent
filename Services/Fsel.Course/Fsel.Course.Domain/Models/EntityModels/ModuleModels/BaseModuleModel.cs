// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ModuleModels
{
    using Fsel.Course.Domain.Models.EntityModels.V1i2;

    public class BaseModuleModel
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public int DisplayNumber { get; set; }
        public int OpenOrder { get; set; }
        public Guid OriginalId { get; set; }

        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? InstructionContent { get; set; }
        public string? Thumbnail { get; set; }
        public Guid ObjectId { get; set; }
        public ResultModel? Result { get; set; }
    }
}
