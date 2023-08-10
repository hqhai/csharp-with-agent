// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Core.Base.BaseModels;

    public class LogActionModel : BaseModel
    {
        public string? FunctionType { get; set; }

        public string? ActionType { get; set; }

        public string? ChangeData { get; set; }

        public string? RecordIds { get; set; }
    }
}
