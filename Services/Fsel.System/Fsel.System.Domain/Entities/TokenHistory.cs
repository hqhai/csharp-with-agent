// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class TokenHistory : Entity
    {
        private double _volatileToken;
        public Guid? TokenConfigId { get; set; }

        /// <summary>
        /// Token Ban Đầu
        /// </summary>
        public double InitialToken { get; set; }

        /// <summary>
        /// Token đạt được
        /// </summary>
        public double RemainToken { get; set; }

        /// <summary>
        /// Token Tổng
        /// </summary>
        public double VolatileToken
        {
            get
            {
                _volatileToken = InitialToken + (Type == EnumTokenHistoryType.Recevived ? -RemainToken : RemainToken);
                return _volatileToken;
            }
            set { _volatileToken = value; }
        }

        public EnumTokenFeature Feature { get; set; }

        public EnumTokenMission? Mission { get; set; }

        public Guid UserId { get; set; }

        public Guid? ObjectId { get; set; }

        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public EnumTokenHistoryType Type { get; set; }

        public TokenConfig? TokenConfig { get; set; }
    }
}
