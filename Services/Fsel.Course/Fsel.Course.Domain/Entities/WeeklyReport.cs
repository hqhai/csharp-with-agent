// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Helpers;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;

    public class WeeklyReport : Entity
    {
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Email { get; set; }

        public string? ParentEmail { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ParamStr { get; set; }

        [NotMapped]
        public WeeklyReportModel? Param
        {
            get
            {
                return ConvertHelper.Deserialize<WeeklyReportModel>(ParamStr);
            }
            set { ParamStr = ConvertHelper.Serialize(value); }
        }
    }
}
