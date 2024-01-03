// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.UrBox;
    using Fsel.Ordering.Domain.Models.EntityModels.UrBox;
    using Fsel.Shared.Enums;

    public class UrBoxTransaction : Entity
    {
        public EnumUrBoxTransactionStatus Status { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? TransactionId { get; set; }

        public string? RequestBodyStr { get; set; }

        [NotMapped]
        public CreateRedemptionRequestModel? RequestBody
        {
            get { return ConvertHelper.Deserialize<CreateRedemptionRequestModel?>(RequestBodyStr); }
        }

        public string? ResponseBodyStr { get; set; }

        [NotMapped]
        public RedemptionResponseModel? ResponseBody
        {
            get { return ConvertHelper.Deserialize<RedemptionResponseModel?>(ResponseBodyStr); }
        }
    }
}
